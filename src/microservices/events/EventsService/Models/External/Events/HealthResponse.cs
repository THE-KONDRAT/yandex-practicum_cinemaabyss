namespace EventsService.Models.External.Events;

public sealed record HealthResponse(bool Status, DateTimeOffset Timestamp);