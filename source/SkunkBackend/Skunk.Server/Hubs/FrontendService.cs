using Microsoft.AspNetCore.SignalR;
using Skunk.Postgres.Interfaces;

namespace Skunk.Server.Hubs;

public class FrontendService : IFrontendService
{
    private readonly IHubContext<FrontendHub> _hubContext;
    private readonly ILogger<FrontendService> _logger;

    public FrontendService(
        IHubContext<FrontendHub> hubContext,
        ILogger<FrontendService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task SendSensorPayload(SensorPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        await _hubContext.Clients.Groups(HubGroupKeys.Streaming).SendCoreAsync("SensorDataToFrontend",
            [
                payload
            ]);
    }

    public async Task SendSensorStats(IEnumerable<ISensorStats> stats)
    {
        var array = stats as ISensorStats[] ?? stats.ToArray();
        await _hubContext.Clients.Groups(HubGroupKeys.Streaming).SendCoreAsync("SensorStatsToFrontEnd", [array]);
    }
}