using CinemaAbyssApiGateway.Services.Telemetry.Models;

namespace CinemaAbyssApiGateway.Services.Telemetry;

public class DefaultTelemetryPublisher(ILogger<DefaultTelemetryPublisher> logger) : ITelemetryPublisher
{
    public Task PublishAsync(TelemetryEvent @event, string? topic = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("TELEMETRY_EVENT: [{Timestamp}], [{Path}]\r\nTargetEndpoint: {TargetEndpoint}\r\nStatusCode: {StatusCode}\r\nDuration: {DurationMs} ms\r\nError: {Error}\r\n---",
            @event.Timestamp, @event.Path, @event.TargetEndpoint, @event.StatusCode, @event.DurationMs, @event.Error);
        
        return Task.CompletedTask;
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}