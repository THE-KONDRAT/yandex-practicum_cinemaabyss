namespace EventsService.Models.External.Events;

public sealed record EventResponse(string Status, int Partition, long Offset, EventBase Event);