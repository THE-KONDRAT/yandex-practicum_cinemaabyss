namespace CinemaAbyssApiGateway.Services.Telemetry.Models;

public sealed record TelemetryEvent(DateTimeOffset Timestamp, string Path, string? TargetEndpoint,
    int StatusCode, long DurationMs, string? Error = null);