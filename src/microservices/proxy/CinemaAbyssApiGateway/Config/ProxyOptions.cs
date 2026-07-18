namespace CinemaAbyssApiGateway.Config;

public sealed class ProxyOptions
{
    public List<ProxyRouteConfig> Routes { get; set; } = [];
}

public static class ProxyOptionsExtensions
{
    public static string ToRouteString(this List<ProxyRouteConfig> routes)
    {
        return string.Join("\r\n", routes.Select(q => q.ToString()));
    }
}