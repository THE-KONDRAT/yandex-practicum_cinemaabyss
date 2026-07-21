namespace CinemaAbyssApiGateway.Models;

public sealed record HealthResponse(bool Status, DateTimeOffset Timestamp);

public sealed record StateResponse(DateTimeOffset Timestamp, IEnumerable<RouteState> Routes);

public sealed record RouteState(string PathPrefix, string Strategy,
    IEnumerable<EndpointState> ActiveEndpoints, IEnumerable<string> DisabledEndpoints);

public sealed record EndpointState(string Url, int Weight);