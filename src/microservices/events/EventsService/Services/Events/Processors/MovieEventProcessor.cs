using System.Text.Json;
using EventsService.Models.External.Events;

namespace EventsService.Services.Events.Processors;

public sealed class MovieEventProcessor(ILogger<MovieEventProcessor> logger) : IEventProcessor
{
    public string EventType => "movie";

    public Task ProcessAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        var @event = JsonSerializer.Deserialize<MovieEvent>(jsonPayload);
        logger.LogInformation("Processed movie event: {MovieId} - {Title}", @event?.MovieId, @event?.Title);
        return Task.CompletedTask;
    }
}