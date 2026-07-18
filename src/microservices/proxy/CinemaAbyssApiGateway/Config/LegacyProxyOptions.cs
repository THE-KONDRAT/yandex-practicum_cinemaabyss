namespace CinemaAbyssApiGateway.Config;

public class LegacyProxyOptions
{
    public int? Port { get; set; }

    [ConfigurationKeyName("MONOLITH_URL")]
    public string? MonolithUrl { get; set; }
    [ConfigurationKeyName("MOVIES_SERVICE_URL")]
    public string? MoviesServiceUrl { get; set; }
    [ConfigurationKeyName("EVENTS_SERVICE_URL")]
    public string? EventsServiceUrl { get; set; }
    [ConfigurationKeyName("GRADUAL_MIGRATION")]
    public bool? GradualMigration { get; set; }
    [ConfigurationKeyName("MOVIES_MIGRATION_PERCENT")]
    public int? MoviesMigrationPercent { get; set; }
}