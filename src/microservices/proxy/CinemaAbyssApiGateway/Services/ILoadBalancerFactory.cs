using CinemaAbyssApiGateway.Config;
using CinemaAbyssApiGateway.Services.LoadBalancing;

namespace CinemaAbyssApiGateway.Services;

public interface ILoadBalancerFactory
{
    ILoadBalancer Create(ProxyRouteConfig config);
}