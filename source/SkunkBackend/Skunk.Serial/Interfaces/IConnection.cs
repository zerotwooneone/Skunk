namespace Skunk.Serial.Interfaces;

public interface IConnection
{
    Task Open(string portName);
    Task Close();
    event EventHandler<string> ReceivedString;
}