using Microsoft.AspNetCore.SignalR;

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
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }
        await _hubContext.Clients.Groups(HubGroupKeys.Streaming).SendCoreAsync("SensorDataToFrontend",
            [
                payload
            ]);
    }
}