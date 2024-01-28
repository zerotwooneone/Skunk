using Microsoft.AspNetCore.SignalR;

namespace Skunk.Server.Hubs;

public class FrontendHub : Hub<IFrontend>
{
    /// <summary>
    /// A reference to this hub's clients and groups
    /// </summary>
    /// <remarks>This can be used on background threads where this.Clients would fail</remarks>
    private readonly IHubContext<FrontendHub> _hubContext;

    public FrontendHub(IHubContext<FrontendHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    /// <summary>
    /// this is called by the frontend when it expects a response back as a Pong
    /// </summary>
    /// <returns></returns>
    public async Task PingBackend()
    {
        await Clients.All.PongFrontend();
    }

    /// <summary>
    /// this is caled by the frontend when it is responding to a Ping that we send from the backend
    /// </summary>
    /// <returns></returns>
    public async Task PongBackend()
    {
        Console.WriteLine("Got Pong");
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
                    Console.Error.WriteLine(e);
                }
            }
        });
        await base.OnConnectedAsync();
    }

    
}