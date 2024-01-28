using Microsoft.AspNetCore.SignalR;

namespace Skunk.Server.Hubs;

public class ClientService : IClientService
{
    private readonly IHubContext<FrontendHub> _hubContext;

    public ClientService(IHubContext<FrontendHub> hubContext)
    {
        _hubContext = hubContext;
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