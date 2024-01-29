using Microsoft.AspNetCore.SignalR;

namespace Skunk.Server.Hubs;

public class FrontendHub : Hub<IFrontend>
{
    /// <summary>
    /// A reference to this hub's clients and groups
    /// </summary>
    /// <remarks>This can be used on background threads where this.Clients would fail</remarks>
    private readonly IHubContext<FrontendHub> _hubContext;

    private readonly ILogger<FrontendHub> _logger;

    public FrontendHub(
        IHubContext<FrontendHub> hubContext,
        ILogger<FrontendHub> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    /// <summary>
    /// this is called by the frontend when it expects a response back as a Pong
    /// </summary>
    /// <returns></returns>
    public async Task PingBackend()
    {
        _logger.LogInformation("Got ping from front end");
        await Clients.All.PongFrontend();
    }

    /// <summary>
    /// this is caled by the frontend when it is responding to a Ping that we send from the backend
    /// </summary>
    /// <returns></returns>
    public async Task PongBackend()
    {
        _logger.LogInformation("Got Pong");
    }

    private static bool testingStarted = false;
    public override async Task OnConnectedAsync()
    {
        //todo: let the client add themselves to the group
        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroupKeys.Streaming);

        //todo:move this test code to another service
        Task.Factory.StartNew(async () =>
        {
            var random = new Random(1337);
            if (testingStarted)
            {
                return;
            }
            testingStarted = true;
            while (testingStarted)
            {
                await Task.Delay(1000);
                try
                {
                    await _hubContext.Clients.Groups(HubGroupKeys.Streaming).SendCoreAsync("SensorDataToFrontend",
                    [
                        new SensorPayload
                        {
                            Sensors =
                                new Dictionary<string, SensorValues>{
                                {"Dummy Sensor", new SensorValues(new Dictionary<string, float>
                                    {
                                        { "Value -999", random.NextSingle() }
                                    })
                                } }
                        }
                    ]);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error sending dummy data");
                }
            }
        });
        await base.OnConnectedAsync();
    }

    
}