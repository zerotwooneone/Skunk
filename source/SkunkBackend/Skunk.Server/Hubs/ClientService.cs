using Microsoft.AspNetCore.SignalR;

namespace Skunk.Server.Hubs;

public class ClientService : IClientService
{
    private readonly IHubContext<FrontendHub> _hubContext;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IHubContext<FrontendHub> hubContext,
        ILogger<ClientService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task SendFormaldehydeValue(short value)
    {
        await _hubContext.Clients.Groups(HubGroupKeys.Streaming).SendCoreAsync("SensorDataToFrontend",
            [
                new SensorPayload
                {
                    Formaldehyde = value,
                }
            ]);
    }
}