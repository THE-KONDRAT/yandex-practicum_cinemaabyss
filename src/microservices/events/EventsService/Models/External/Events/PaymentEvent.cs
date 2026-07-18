namespace EventsService.Models.External.Events;

public sealed record PaymentEvent : EventBase
{
    public required int PaymentId { get; init; }
    public required int UserId { get; init; }
    public required float Amount { get; init; }
    public required string Status { get; init; }
    public string? MethodType { get; init; }
    
    public override string Type => "payment";
}

public sealed record ErrorResponse(string Error);