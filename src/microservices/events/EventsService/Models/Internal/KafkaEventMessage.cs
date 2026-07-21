namespace EventsService.Models.Internal;

public sealed record KafkaEventMessage(string EventId, string EventType, DateTimeOffset Timestamp,
    string PayloadJson, string? SourceIp);