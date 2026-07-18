using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using EventsService.Config;
using EventsService.Models.External.Events;

namespace EventsService.Services;

public class KafkaProducer(IProducer<Null, string> producer, KafkaTopicsConfig topicsConfig, ILogger<KafkaProducer> logger) : IEventProducer
{
    public async Task<EventResponse> PublishAsync(EventBase @event, CancellationToken cancellationToken = default)
    {
        var topic = topicsConfig.ResolveTopic(@event.Type);
        var json = JsonSerializer.Serialize(@event, @event.GetType());
        
        var result = await producer.ProduceAsync(topic, new Message<Null, string>
        {
            Value = json,
            Headers = new Headers
            {
                { "event-type", Encoding.UTF8.GetBytes(@event.Type) },
                { "event-id", Encoding.UTF8.GetBytes(@event.Id) }
            }
        }, cancellationToken);
        
        logger.LogInformation("Published event: {EventId} - {Topic}", @event.Id, topic);

        return new EventResponse(Status: "success", Partition: result.Partition, Offset: result.Offset.Value, Event: @event);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}