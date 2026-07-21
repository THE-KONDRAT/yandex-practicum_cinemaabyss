using EventsService.Models.External.Events;

namespace EventsService.Services;

public interface IEventProducer : IDisposable, IAsyncDisposable
{
    public Task<EventResponse> PublishAsync(EventBase @event, CancellationToken cancellationToken = default);
}