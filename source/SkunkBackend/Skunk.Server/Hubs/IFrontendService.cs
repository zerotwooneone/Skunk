using Skunk.Postgres.Interfaces;

namespace Skunk.Server.Hubs;

public interface IFrontendService
{
    Task SendSensorPayload(SensorPayload payload);
    Task SendSensorStats(IEnumerable<ISensorStats> stats);
}