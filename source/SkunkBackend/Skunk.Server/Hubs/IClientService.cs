namespace Skunk.Server.Hubs;

public interface IClientService
{
    Task SendFormaldehydeValue(short value);
}