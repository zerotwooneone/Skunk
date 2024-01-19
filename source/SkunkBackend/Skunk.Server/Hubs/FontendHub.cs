using Microsoft.AspNetCore.SignalR;

namespace Skunk.Server.Hubs
{
    public class FontendHub : Hub<IFrontEnd>
    {
        public FontendHub() { }

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
    }
}
