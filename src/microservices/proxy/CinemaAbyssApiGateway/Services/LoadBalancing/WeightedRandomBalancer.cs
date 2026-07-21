using System.Security.Cryptography;

namespace CinemaAbyssApiGateway.Services.LoadBalancing;

public class WeightedRandomBalancer : ILoadBalancer
{
    private readonly Endpoint[] _endpoints;
    private readonly int _totalWeight;
    private readonly ILogger _logger;

    public WeightedRandomBalancer(IReadOnlyDictionary<string, int> servicesWithWeights, ILogger<WeightedRandomBalancer> logger)
    {
        var active = servicesWithWeights
            .Where(x => x.Value > 0)
            .ToList();
        _endpoints = new Endpoint[active.Count];
        
        var cumulative = 0;
        for (var i = 0; i < active.Count; i++)
        {
            cumulative += active[i].Value;
            _endpoints[i] = new Endpoint(active[i].Key, cumulative);
        }
        
        _totalWeight = cumulative;
        _logger = logger;
    }
    
    public BalanceResult SelectEndpoint(HttpContext context)
    {
        if (_totalWeight == 0)
        {
            _logger.LogDebug("Service is disabled");
            return BalanceResult.NoActiveEndpoints();
        }
        var rnd = RandomNumberGenerator.GetInt32(0, _totalWeight);
        var idx = BinarySearchUpperBound(_endpoints, rnd);
        var url = _endpoints[idx].Url;
        _logger.LogDebug("Random value: {RandomValue}, total weight: {TotalWeight}, index: {Index}, target: {Target}", 
            rnd, _totalWeight, idx, url);
        return BalanceResult.Success(url);
    }
    
    private static int BinarySearchUpperBound(ReadOnlySpan<Endpoint> endpoints, int target)
    {
        int lo = 0, hi = endpoints.Length - 1;
        
        while (lo < hi)
        {
            var mid = lo + (hi - lo) / 2;
            if (endpoints[mid].CumulativeWeight <= target)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;   
            }
        }
        
        return lo;
    }
    
    private readonly struct Endpoint(string url, int cumulativeWeight)
    {
        public readonly string Url = url;
        public readonly int CumulativeWeight = cumulativeWeight;
    }
}