using System.Text.Json.Serialization;
using CinemaAbyssApiGateway.Config;
using CinemaAbyssApiGateway.Models;
using CinemaAbyssApiGateway.Services.Telemetry.Models;

namespace CinemaAbyssApiGateway;

[JsonSerializable(typeof(TelemetryEvent))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(StateResponse))]
[JsonSerializable(typeof(ProxyOptions))]
[JsonSerializable(typeof(ProxyRouteConfig))]
[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class AppJsonContext : JsonSerializerContext
{
}