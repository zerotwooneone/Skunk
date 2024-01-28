namespace Skunk.Serial.Interfaces;

public interface IConnectionFactory
{
    IEnumerable<string> GetPortNames();
    bool TryCreate(string portName, out IConnection? connection);
}