namespace EventsService.Services;

public interface IEventConsumer : IDisposable, IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}