namespace Skunk.Serial.Interfaces;

/// <summary>
/// Represents an open serial connection
/// </summary>
public interface IConnection : IDisposable
{    
    event EventHandler<string> ReceivedString;
}