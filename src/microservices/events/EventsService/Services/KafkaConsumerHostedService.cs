using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventsService.Config;

namespace EventsService.Services;

public class KafkaConsumerHostedService(IEventConsumer consumer, ConsumerConfig config,
    KafkaTopicsConfig topicsConfig, ILogger<KafkaConsumerHostedService> logger) : IHostedService, IAsyncDisposable
{
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        EnsureTopicsExist(config.BootstrapServers, topicsConfig.AllTopics, logger);
        
        logger.LogInformation("Topics ready, starting consumer...");
        
        await consumer.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) 
        => consumer.StopAsync(cancellationToken);

    private static void EnsureTopicsExist(string bootstrapServers, string[] topicNames, ILogger logger)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        }).Build();

        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

            var topicsToCreate = topicNames
                .Where(t => !existingTopics.Contains(t))
                .Select(t => new TopicSpecification
                {
                    Name = t,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                })
                .ToList();

            if (topicsToCreate.Count == 0) return;

            adminClient.CreateTopicsAsync(topicsToCreate).Wait();
            logger.LogInformation("Created topics: {CreatedTopics}", string.Join(", ", topicsToCreate.Select(t => t.Name)));
        }
        catch (CreateTopicsException ex)
        {
            if (ex.Results.Any(r => r.Error.Code != ErrorCode.TopicAlreadyExists)) throw;
            logger.LogInformation(ex, "Topics already exist");
        }
        catch (Exception ex)
        {
            logger.LogError("Warning: Failed to ensure topics exist: {error}", ex.Message);
        }
    }

    public ValueTask DisposeAsync() => consumer.DisposeAsync();
}