namespace CinemaAbyssApiGateway.Config;

public enum BalancingStrategy
{
    RandomPercent,
    RoundRobin,
    HashBased,
    HeaderBased
}