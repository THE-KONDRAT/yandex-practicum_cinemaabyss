using System.Text.Json;
using EventsService.Models.External.Events;

namespace EventsService.Services.Events.Processors;

public sealed class UserEventProcessor(ILogger<UserEventProcessor> logger) : IEventProcessor
{
    public string EventType => "user";

    public Task ProcessAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<UserEvent>(jsonPayload);
        
        if (@event is null)
        {
            logger.LogWarning("Failed to deserialize user event");
            return Task.CompletedTask;
        }

        logger.LogInformation("User event: {Action} user {UserId} ({Username}) at {Timestamp}",
            @event.Action, @event.UserId, @event.Username, @event.Timestamp);

        return Task.CompletedTask;
    }
}