namespace CinemaAbyssApiGateway.Config;

public static class ProxyConfigAdapter
{
    public static List<ProxyRouteConfig> FromLegacy(LegacyProxyOptions legacy)
    {
        var routes = string.IsNullOrWhiteSpace(legacy.MonolithUrl)
        ? []
        : new List<ProxyRouteConfig>
        {
            new ProxyRouteConfig
            {
                PathPrefix = "/",
                Strategy = BalancingStrategy.RoundRobin,
                NoActiveEndpointsStatusCode = 503,
                EndpointsWithWeights = new Dictionary<string, int>
                {
                    [legacy.MonolithUrl] = 1
                }
            }
        };
        
        if (legacy.GradualMigration == true
            && !string.IsNullOrWhiteSpace(legacy.MonolithUrl) && !string.IsNullOrWhiteSpace(legacy.MoviesServiceUrl)
            && legacy.MoviesMigrationPercent.HasValue)
        {
            routes.Add(new ProxyRouteConfig
            {
                PathPrefix = "/api/movies",
                Strategy = BalancingStrategy.RandomPercent,
                NoActiveEndpointsStatusCode = 503,
                EndpointsWithWeights = new Dictionary<string, int>
                {
                    [legacy.MoviesServiceUrl] = legacy.MoviesMigrationPercent.Value,
                    [legacy.MonolithUrl] = 100 - legacy.MoviesMigrationPercent.Value
                }
            });
        }
        else if (!string.IsNullOrWhiteSpace(legacy.MoviesServiceUrl))
        {
            routes.Add(new ProxyRouteConfig
            {
                PathPrefix = "/api/movies",
                Strategy = BalancingStrategy.RoundRobin,
                NoActiveEndpointsStatusCode = 503,
                EndpointsWithWeights = new Dictionary<string, int>
                {
                    [legacy.MoviesServiceUrl] = 1
                }
            });
        }

        if (!string.IsNullOrWhiteSpace(legacy.EventsServiceUrl))
        {
            routes.Add(new ProxyRouteConfig
            {
                PathPrefix = "/api/events",
                Strategy = BalancingStrategy.RoundRobin,
                NoActiveEndpointsStatusCode = 503,
                EndpointsWithWeights = new Dictionary<string, int>
                {
                    [legacy.EventsServiceUrl] = 1
                }
            });
        }

        return routes;
    }
}