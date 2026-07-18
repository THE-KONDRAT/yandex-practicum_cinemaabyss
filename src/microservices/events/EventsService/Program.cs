using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using EventsService;
using EventsService.Config;
using EventsService.Models.External.Events;
using EventsService.Services;
using EventsService.Services.Events;
using EventsService.Services.Events.Processors;
using Microsoft.AspNetCore.Mvc;

var portEnv = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(portEnv, out var port) && port is > 0 and < 65535)
{
    Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", port.ToString());
}

var builder = WebApplication.CreateBuilder(args);

var bootstrapServers = builder.Configuration["KAFKA_BROKERS"]
                       ?? throw new InvalidOperationException("KAFKA_BROKERS not set");

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    
    return new KafkaTopicsConfig
    {
        MovieTopic = config["KAFKA_TOPIC_MOVIE"] ?? "events.movie",
        UserTopic = config["KAFKA_TOPIC_USER"] ?? "events.user",
        PaymentTopic = config["KAFKA_TOPIC_PAYMENT"] ?? "events.payment"
    };
});

builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = bootstrapServers,
        CompressionType = CompressionType.Lz4,
        Acks = Acks.Leader,
        LingerMs = 5,
        ClientId = "events-service-producer"
    };
    return new ProducerBuilder<Null, string>(config).Build();
});
builder.Services.AddScoped<IEventProducer, KafkaProducer>();

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = bootstrapServers,
    GroupId = builder.Configuration["KAFKA_CONSUMER_GROUP"] ?? "events-service-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false,
    EnableAutoOffsetStore = false,
    MaxPollIntervalMs = 300000,
    SessionTimeoutMs = 45000,
    ClientId = "events-service-consumer"
};

builder.Services.AddSingleton<IEventConsumer>(sp => new KafkaConsumer(consumerConfig, sp.GetRequiredService<KafkaTopicsConfig>(),
    sp.GetRequiredService<IEventHandler>(),
    sp.GetRequiredService<ILogger<KafkaConsumer>>()));

builder.Services.AddSingleton(consumerConfig);

// --- Event Handlers ---
builder.Services.AddSingleton<IEventHandler, DefaultEventHandler>();
builder.Services.AddSingleton<IEventProcessor, MovieEventProcessor>();
builder.Services.AddSingleton<IEventProcessor, UserEventProcessor>();
builder.Services.AddSingleton<IEventProcessor, PaymentEventProcessor>();

builder.Services.AddHostedService<KafkaConsumerHostedService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.MapGroup("/api/events")
    .MapEventsApi()
    .WithGroupName("events");

app.Run();

namespace EventsService
{
    public static class RouteGroupBuilderExtensions
    {
        public static RouteGroupBuilder MapEventsApi(this RouteGroupBuilder group)
        {
            group.MapGet("/health", () =>
                {
                    var response = new HealthResponse(true, DateTimeOffset.UtcNow);
                    return Results.Json(response);
                })
                .WithName("HealthCheck");
            group.MapPost("/movie", async ([FromBody] MovieEvent @event, [FromServices] IEventProducer producer) =>
            {
                var result = await producer.PublishAsync(@event);
                return Results.Json(result, statusCode: 201);
            });
            group.MapPost("/user", async ([FromBody] UserEvent @event, [FromServices] IEventProducer producer) =>
            {
                var result = await producer.PublishAsync(@event);
                return Results.Json(result, statusCode: 201);
            });
            group.MapPost("/payment", async ([FromBody] PaymentEvent @event, [FromServices] IEventProducer producer) =>
            {
                var result = await producer.PublishAsync(@event);
                return Results.Json(result, statusCode: 201);
            });

            return group;
        }
    }
}