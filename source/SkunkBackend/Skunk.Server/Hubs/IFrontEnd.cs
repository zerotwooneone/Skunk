namespace Skunk.Server.Hubs
{
    /// <summary>
    /// This is the interface of the methods which we can call from the backend to the frontend
    /// </summary>
    public interface IFrontend
    {
        /// <summary>
        /// This requests ALL clients send us a Pong
        /// </summary>
        /// <returns></returns>
        Task PingFrontend();

        /// <summary>
        /// This sends ALL clients a Pong
        /// </summary>
        /// <returns></returns>
        Task PongFrontend();

        Task SensorDataToFrontend(SensorPayload sensorPayload);
    }
}