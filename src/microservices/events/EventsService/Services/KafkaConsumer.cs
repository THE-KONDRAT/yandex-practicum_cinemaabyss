using System.Text;
using Confluent.Kafka;
using EventsService.Config;
using EventsService.Services.Events;

namespace EventsService.Services;

public class KafkaConsumer : IEventConsumer
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly IEventHandler _eventHandler;
    private readonly KafkaTopicsConfig  _topicsConfig;
    private Task? _consumeTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public KafkaConsumer(ConsumerConfig config, KafkaTopicsConfig  topicsConfig,
        IEventHandler eventHandler, ILogger<KafkaConsumer> logger)
    {
        _topicsConfig = topicsConfig;
        _eventHandler = eventHandler;
        _logger = logger;
        
        _consumer = new ConsumerBuilder<Null, string>(config)
            .SetErrorHandler((_, error) => 
                _logger.LogError("Kafka error: {Reason}", error.Reason))
            .SetPartitionsAssignedHandler((_, partitions) => 
                _logger.LogInformation("Assigned partitions: {Partitions}", 
                    string.Join(", ", partitions.Select(p => p.Partition.Value))))
            .Build();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _consumeTask = Task.Run(() => ConsumeLoop(_cancellationTokenSource.Token), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topicsConfig.AllTopics);
        var messageCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(cancellationToken);
                
                if (result.IsPartitionEOF)
                {
                    _logger.LogDebug("Reached end of partition");
                    continue;
                }

                await HandleMessage(result, cancellationToken);

                _consumer.StoreOffset(result);
                messageCount++;

                if (messageCount % 100 != 0) continue;

                _consumer.Commit();
                _logger.LogInformation("Committed batch of {Count} messages", messageCount);
            }
            catch (OperationCanceledException)
            {
                _consumer.Commit();
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Consume error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in consume loop");
            }
        }
    }

    private async Task HandleMessage(ConsumeResult<Null, string> result, CancellationToken cancellationToken)
    {
        var message = result.Message;
        var eventType = message.Headers.TryGetLastBytes("event-type", out var typeBytes)
            ? Encoding.UTF8.GetString(typeBytes)
            : "unknown";

        _logger.LogInformation("Received {EventType} from {Topic}[{Partition}]@{Offset}",
            eventType, result.TopicPartition.Topic, result.TopicPartition.Partition.Value, result.Offset.Value);

        await _eventHandler.HandleAsync(eventType, message.Value, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cancellationTokenSource != null)
            await _cancellationTokenSource.CancelAsync();
        
        if (_consumeTask is not null)
        {
            try
            {
                await _consumeTask.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Consumer stop timed out");
            }
        }

        _consumer.Close();
    }

    public ValueTask DisposeAsync()
    {
        _consumer.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _consumer.Dispose();
        GC.SuppressFinalize(this);
    }
}