namespace Skunk.Serial.Interfaces;

public interface IConnectionFactory
{
    Task<IConnection> GetConnection();
}