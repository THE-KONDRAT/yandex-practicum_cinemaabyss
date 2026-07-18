namespace EventsService.Services.Events;

public interface IEventHandler
{
    Task HandleAsync(string eventType, string jsonPayload, CancellationToken cancellationToken = default);
}

public interface IEventProcessor
{
    string EventType { get; }
    Task ProcessAsync(string jsonPayload, CancellationToken cancellationToken = default);
}