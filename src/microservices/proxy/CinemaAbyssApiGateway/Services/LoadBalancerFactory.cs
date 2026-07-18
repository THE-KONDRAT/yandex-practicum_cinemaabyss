using CinemaAbyssApiGateway.Config;
using CinemaAbyssApiGateway.Services.LoadBalancing;

namespace CinemaAbyssApiGateway.Services;

public class LoadBalancerFactory(ILoggerFactory loggerFactory) : ILoadBalancerFactory
{
    public ILoadBalancer Create(ProxyRouteConfig config) => config.Strategy switch
    {
        BalancingStrategy.RandomPercent => new WeightedRandomBalancer(config.EndpointsWithWeights, loggerFactory.CreateLogger<WeightedRandomBalancer>()),
        BalancingStrategy.RoundRobin    => new RoundRobinBalancer(config.EndpointsWithWeights.Keys),
        BalancingStrategy.HashBased     => throw new NotImplementedException(), //TODO: implement this
        BalancingStrategy.HeaderBased   => new HeaderBasedBalancer(config.HeaderMappings, config.HeaderName!),
        _ => throw new NotSupportedException()
    };
}