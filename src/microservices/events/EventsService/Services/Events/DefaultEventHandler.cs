namespace EventsService.Services.Events;

public sealed class DefaultEventHandler(
    ILogger<DefaultEventHandler> logger,
    IEnumerable<IEventProcessor> processors) : IEventHandler
{
    private readonly Dictionary<string, IEventProcessor> _processors = processors
        .ToDictionary(p => p.EventType, StringComparer.OrdinalIgnoreCase);

    public Task HandleAsync(string eventType, string jsonPayload, CancellationToken cancellationToken = default)
    {
        if (_processors.TryGetValue(eventType, out var processor))
        {
            return processor.ProcessAsync(jsonPayload, cancellationToken);
        }

        logger.LogWarning("No processor found for event type: {EventType}", eventType);
        return Task.CompletedTask;
    }
}