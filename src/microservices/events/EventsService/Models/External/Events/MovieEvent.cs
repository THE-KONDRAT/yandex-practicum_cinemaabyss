namespace EventsService.Models.External.Events;

public sealed record MovieEvent : EventBase
{
    public required int MovieId { get; init; }
    public required string Title { get; init; }
    public required string Action { get; init; }
    public int? UserId { get; init; }
    public float? Rating { get; init; }
    public string[]? Genres { get; init; }
    public string? Description { get; init; }
    
    public override string Type => "movie";
}