namespace EventsService.Models.External.Events;

public sealed record UserEvent : EventBase
{
    public required int UserId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public required string Action { get; init; }
    
    public override string Type => "user";
}