namespace CinemaAbyssApiGateway.Config;

public sealed class ProxyRouteConfig
{
    public string PathPrefix { get; set; } = string.Empty;
    public BalancingStrategy Strategy { get; set; }
    public int NoActiveEndpointsStatusCode { get; set; } = 503;
    
    public Dictionary<string, int> EndpointsWithWeights { get; set; } = [];

    public string? HeaderName { get; set; }
    public Dictionary<string, string> HeaderMappings { get; set; } = [];
    public string? DefaultEndpoint { get; set; }
    
    public override string ToString() =>
        string.Concat(PathPrefix, " – |", EndpointsWithWeightsToString(), "| [", Strategy, "]");

    private string EndpointsWithWeightsToString()
    {
        var endpoints = new List<string>();
        if (Strategy == BalancingStrategy.HeaderBased)
        {
            endpoints.AddRange(HeaderMappings.OrderBy(q => q.Value)
                .Select(q => Strategy switch
                {
                    BalancingStrategy.RandomPercent => EndpointToString(q.Key, q.Value),
                    BalancingStrategy.RoundRobin or BalancingStrategy.HashBased => EndpointToString(q.Key),
                    _ => throw new ArgumentOutOfRangeException()
                }));
        }
        else
        {
             endpoints.AddRange(EndpointsWithWeights.OrderBy(q => q.Value)
                 .Select(q => Strategy switch
                 {
                     BalancingStrategy.RandomPercent => EndpointToString(q.Key, q.Value),
                     BalancingStrategy.RoundRobin or BalancingStrategy.HashBased => EndpointToString(q.Key),
                     _ => throw new ArgumentOutOfRangeException()
                 })); 
        }

        return string.Join("; ", endpoints);
    }

    private string EndpointToString(string endpoint, int percentage)
    {
        return string.Concat(endpoint, " -> ", percentage, "%");
    }

    private string EndpointToString(string endpoint, string header)
    {
        return string.IsNullOrWhiteSpace(HeaderName)
            ? string.Empty
            : string.Concat(endpoint, " if [", HeaderName, "] == ", header);
    }

    private string EndpointToString(string endpoint)
    {
        return Strategy switch
        {
            BalancingStrategy.RoundRobin => endpoint,
            BalancingStrategy.HashBased => endpoint,
            _ => throw new NotImplementedException(Strategy.ToString())
        };
    }
}