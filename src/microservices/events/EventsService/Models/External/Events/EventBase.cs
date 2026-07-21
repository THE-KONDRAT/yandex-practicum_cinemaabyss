namespace EventsService.Models.External.Events;

public abstract record EventBase
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public abstract string Type { get; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}