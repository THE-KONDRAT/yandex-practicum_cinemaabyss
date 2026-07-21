using System.Text.Json;
using EventsService.Models.External.Events;

namespace EventsService.Services.Events.Processors;

public sealed class PaymentEventProcessor(ILogger<PaymentEventProcessor> logger) : IEventProcessor
{
    public string EventType => "payment";

    public Task ProcessAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<PaymentEvent>(jsonPayload);
        
        if (@event is null)
        {
            logger.LogWarning("Failed to deserialize payment event");
            return Task.CompletedTask;
        }

        logger.LogInformation("Payment event: {Status} payment {PaymentId} for user {UserId}, amount {Amount} at {Timestamp}",
            @event.Status, @event.PaymentId, @event.UserId, @event.Amount, @event.Timestamp);

        return Task.CompletedTask;
    }
}