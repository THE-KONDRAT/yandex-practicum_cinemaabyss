namespace CinemaAbyssApiGateway.Services.LoadBalancing;

public class RoundRobinBalancer(IEnumerable<string> endpoints) : ILoadBalancer
{
    private readonly string[] _endpoints = endpoints.ToArray();
    private long _counter;

    public BalanceResult SelectEndpoint(HttpContext context)
    {
        if (_endpoints.Length == 0)
            return BalanceResult.NoActiveEndpoints();
        var idx = (int)(Interlocked.Increment(ref _counter) % _endpoints.Length);
        return BalanceResult.Success(_endpoints[idx]);
    }
}