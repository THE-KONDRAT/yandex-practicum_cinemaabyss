namespace EventsService.Config;

public sealed class KafkaTopicsConfig
{
    public string MovieTopic { get; set; } = "events.movie";
    public string UserTopic { get; set; } = "events.user";
    public string PaymentTopic { get; set; } = "events.payment";

    public string ResolveTopic(string eventType) => eventType.ToLowerInvariant() switch
    {
        "movie" => MovieTopic,
        "user" => UserTopic,
        "payment" => PaymentTopic,
        _ => throw new ArgumentException($"Unknown event type: {eventType}")
    };

    public string[] AllTopics => [MovieTopic, UserTopic, PaymentTopic];
}