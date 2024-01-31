namespace Skunk.Server.Hubs;

public interface IFrontendService
{
    Task SendSensorPayload(SensorPayload payload);
}