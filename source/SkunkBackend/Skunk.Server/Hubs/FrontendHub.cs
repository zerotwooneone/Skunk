using Microsoft.AspNetCore.SignalR;

namespace Skunk.Server.Hubs
{
    public class FrontendHub : Hub<IFrontend>
    {
        private static bool testingStarted = false;
        
        /// <summary>
        /// this is called by the frontend when it expects a response back as a Pong
        /// </summary>
        /// <returns></returns>
        public async Task PingBackend()
        {
            await Clients.All.PongFrontend();

            Task.Factory.StartNew(async () =>
            {
                if (testingStarted)
                {
                    return;
                }
                testingStarted = true;
                while (testingStarted)
                {
                    await Task.Delay(1000);
                    Clients.All.SensorDataToFrontend(
                        new SensorPayload
                        {
                            Sensors =
                        new Dictionary<string, SensorValues>{
                            {"Dummy Sensor", new SensorValues(new Dictionary<string, float>
                            {
                                { "Value -999", -999f }
                             })
                        } }
                        });
                }
            });
        }

        /// <summary>
        /// this is caled by the frontend when it is responding to a Ping that we send from the backend
        /// </summary>
        /// <returns></returns>
        public async Task PongBackend()
        {
            Console.WriteLine("Got Pong");
        }
    }
}
