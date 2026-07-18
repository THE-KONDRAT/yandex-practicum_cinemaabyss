using CinemaAbyssApiGateway.Services.Telemetry.Models;

namespace CinemaAbyssApiGateway.Services.Telemetry;

public interface ITelemetryPublisher : IDisposable, IAsyncDisposable
{
    Task PublishAsync(TelemetryEvent @event, string? topic = null, CancellationToken cancellationToken = default);
}