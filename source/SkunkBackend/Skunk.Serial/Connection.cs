using System.IO.Ports;
using Microsoft.Extensions.Logging;
using Skunk.Serial.Interfaces;

namespace Skunk.Serial;

public class Connection : IConnection
{
    public event EventHandler<string>? ReceivedString;

    private readonly ILogger<Connection> _logger;
    private readonly SerialPort _serialPort;

    public Connection(
        ILogger<Connection> logger,
        SerialPort serialPort)
    {
        _logger = logger;
        _serialPort = serialPort;

        void OnSerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
        {
            if (serialPort.BytesToRead == 0)
            {
                return;
            }

            var existingString = serialPort
                .ReadLine()
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            RaiseReceivedString(existingString);
        }

        serialPort.DataReceived += OnSerialPortOnDataReceived;

        //todo: sense disconnect

        serialPort.ErrorReceived += (s, e) =>
        {
            _logger.LogError("Serial Error Received. ErrorType: {eventInt} - {eventType}", (int)e.EventType, e.EventType.ToString());
            try
            {
                //try to clear the buffer
                serialPort.ReadExisting();
            }
            catch
            {
                _logger.LogError("Error trying to clear buffer on serial error. Bytes:{bytes}", serialPort.BytesToRead);
            }
        };
    }     

    public void Dispose()
    {
        if(_serialPort.IsOpen) {
            try
            {
                _serialPort.Close();
            }
            catch
            {
                //empty
            }
        }
        try
        {
            _serialPort.Dispose();
        }
        catch
        {
            //empty
        }
    }

    protected void RaiseReceivedString(string message)
    {
        ReceivedString?.Invoke(this, message);
    }
}
