namespace CinemaAbyssApiGateway.Services.LoadBalancing;

public class HeaderBasedBalancer : ILoadBalancer
{
    private readonly Dictionary<string, string> _headerToEndpoint;
    private readonly string _headerName;
    private readonly string? _defaultEndpoint;
    
    public HeaderBasedBalancer(IReadOnlyDictionary<string, string> headerToEndpoint, string headerName, string? defaultEndpoint = null)
    {
        _headerName = headerName;
        _defaultEndpoint = defaultEndpoint;
        _headerToEndpoint = new Dictionary<string, string>(headerToEndpoint, StringComparer.OrdinalIgnoreCase);
    }
    
    public BalanceResult SelectEndpoint(HttpContext context)
    {
        var headerValue = context.Request.Headers[_headerName].FirstOrDefault();

        return BalanceResult.Success(headerValue is not null && _headerToEndpoint.TryGetValue(headerValue, out var endpoint)
            ? endpoint
            : _defaultEndpoint);
    }
}