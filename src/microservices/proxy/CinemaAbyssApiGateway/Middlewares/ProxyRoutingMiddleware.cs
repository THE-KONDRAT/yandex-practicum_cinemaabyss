using System.Collections.Concurrent;
using System.Diagnostics;
using CinemaAbyssApiGateway.Config;
using CinemaAbyssApiGateway.Services;
using CinemaAbyssApiGateway.Services.LoadBalancing;
using CinemaAbyssApiGateway.Services.Telemetry;
using CinemaAbyssApiGateway.Services.Telemetry.Models;
using Microsoft.Extensions.Options;

namespace CinemaAbyssApiGateway.Middlewares;

public sealed class ProxyRoutingMiddleware(RequestDelegate next, ILoadBalancerFactory factory, IOptions<ProxyOptions> proxyOptions,
    ITelemetryPublisher telemetry, HttpClient httpClient, IHostEnvironment env, ILogger <ProxyRoutingMiddleware> logger) : ProxyBase(httpClient)
{
    private readonly ConcurrentDictionary<string, ILoadBalancer> _balancers = [];

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "";
        var config = ResolveConfig(path);
        TelemetryEvent? telemetryEvent = null;

        try
        {
            if (config is null)
            {
                await next(context);
                telemetryEvent = new TelemetryEvent(DateTimeOffset.UtcNow, path, null,
                    context.Response.StatusCode, sw.ElapsedMilliseconds);
                return;
            }

            var balancer = _balancers.GetOrAdd(path, factory.Create(config));
            var target = balancer.SelectEndpoint(context);

            if (!target.HasEndpoint)
            {
                logger.LogDebug("No target endpoint was found.");
                context.Response.StatusCode = config.NoActiveEndpointsStatusCode;
                telemetryEvent = new TelemetryEvent(DateTimeOffset.UtcNow, path, null,
                    config.NoActiveEndpointsStatusCode, sw.ElapsedMilliseconds);
                return;
            }
    
            logger.LogDebug("Target endpoint was found: {TargetEndpoint}",  target.Endpoint);
            await ProxyRequest(context, target.Endpoint!);

            telemetryEvent = new TelemetryEvent(DateTimeOffset.UtcNow, path, target.Endpoint,
                context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            
            context.Response.StatusCode = 502;
            telemetryEvent = new TelemetryEvent(DateTimeOffset.UtcNow, path, null,
                context.Response.StatusCode, sw.ElapsedMilliseconds, ex.Message);
            throw;
        }
        finally
        {
            if (telemetryEvent is not null)
                _ = telemetry.PublishAsync(telemetryEvent);
        }
    }
    
    private ProxyRouteConfig? ResolveConfig(string path)
    {
        var routes = proxyOptions.Value.Routes;
        var result = routes
            .Where(r => path.StartsWith(r.PathPrefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.PathPrefix.Length)
            .FirstOrDefault();
        if (!env.IsDevelopment())
        {
            return result;
        }
        
        logger.LogTrace("Resolving config: {path} from routes:\r\n{routes}", path, routes.ToRouteString());

        if (result == null)
        {
            logger.LogDebug("Route config was not found");
        }
        else
        {
            logger.LogDebug("Route config was found: {RouteConfig}", result.ToString());
        }
        return result;
    }
}