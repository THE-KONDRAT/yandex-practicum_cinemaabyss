using System.Text.Json;
using CinemaAbyssApiGateway;
using CinemaAbyssApiGateway.Config;
using CinemaAbyssApiGateway.Middlewares;
using CinemaAbyssApiGateway.Models;
using CinemaAbyssApiGateway.Services;
using CinemaAbyssApiGateway.Services.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var portEnv = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(portEnv, out var port) && port is > 0 and < 65535)
{
    Environment.SetEnvironmentVariable("ASPNETCORE_HTTP_PORTS", port.ToString());
}

var builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.AddJsonFile("routes.json", optional: true, reloadOnChange: true);

builder.Services.AddSingleton<LegacyProxyOptions>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var config = sp.GetRequiredService<IConfiguration>();
    var legacy = new LegacyProxyOptions();
    config.Bind(legacy);
    return legacy;
});

builder.Services.AddSingleton<IOptions<ProxyOptions>>(sp =>
{
    var legacy = sp.GetRequiredService<LegacyProxyOptions>();
    var logger = sp.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Legacy options: {@Legacy}", legacy);
    
    var options = new ProxyOptions { Routes = ProxyConfigAdapter.FromLegacy(legacy) };
    logger.LogInformation("Routes: {@Routes}", options.Routes.ToRouteString());
    
    return Options.Create(options);
});

builder.Services.AddSingleton<HttpClient>(sp => 
{
    var handler = new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        EnableMultipleHttp2Connections = true
    };
    return new HttpClient(handler);
});

// -- TELEMETRY
builder.Services.AddSingleton<ITelemetryPublisher, DefaultTelemetryPublisher>();
// --

builder.Services.AddSingleton<ILoadBalancerFactory, LoadBalancerFactory>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
});

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation("Shutdown requested, draining connections...");
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
        logger.LogDebug("Request cancelled due to shutdown");
        context.Response.StatusCode = 499;
    }
});

app.UseMiddleware<ProxyRoutingMiddleware>();

app.MapGet("/health", () =>
{
    var response = new HealthResponse(true, DateTimeOffset.UtcNow);
    return Results.Json(response, AppJsonContext.Default.HealthResponse, statusCode: 200);
})
.WithName("HealthCheck");

app.MapGet("/state", ([FromServices] IOptions<ProxyOptions> options) =>
{
    var response = new StateResponse(
        DateTimeOffset.UtcNow,
        options.Value.Routes.Select(r => new RouteState(
            r.PathPrefix,
            r.Strategy.ToString(),
            r.EndpointsWithWeights.Where(e => e.Value > 0)
                .Select(e => new EndpointState(e.Key, e.Value)),
            r.EndpointsWithWeights.Where(e => e.Value == 0).Select(e => e.Key)
        ))
    );
    return Results.Json(response, AppJsonContext.Default.StateResponse);
});

app.Run();