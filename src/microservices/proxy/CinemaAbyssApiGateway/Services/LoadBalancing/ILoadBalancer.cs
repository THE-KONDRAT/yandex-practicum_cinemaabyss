namespace CinemaAbyssApiGateway.Services.LoadBalancing;

public interface ILoadBalancer
{
    BalanceResult SelectEndpoint(HttpContext context);
}

public readonly record struct BalanceResult
{
    public string? Endpoint { get; init; }
    public bool HasEndpoint => Endpoint is not null;

    public static BalanceResult Success(string? endpoint) => new()
    {
        Endpoint = string.IsNullOrWhiteSpace(endpoint) ? null : endpoint
    };

    public static BalanceResult NoActiveEndpoints() => new();
}