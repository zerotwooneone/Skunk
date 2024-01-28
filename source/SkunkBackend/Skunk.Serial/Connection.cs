using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;
using Skunk.Serial.Interfaces;

namespace Skunk.Serial;

public class Connection : IConnection
{
    private readonly ILogger<Connection> _logger;
    public Connection(ILogger<Connection> logger)
    {
        _logger = logger;
    }
    public IEnumerable<string> GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    public async Task Open(string portName)
    {
        if(string.IsNullOrWhiteSpace(portName))
        {
            _logger.LogError("Portname was null or blank");
            return;
        }
        var portNames = GetPortNames();
        _logger.LogDebug("Found port names {names}", string.Join(",", portNames));
        
        var foundPortName = portNames.FirstOrDefault(p=>p.Equals(portName, StringComparison.OrdinalIgnoreCase));

        if(string.IsNullOrWhiteSpace(foundPortName))
        {
            _logger.LogError("Portname was not found {portName}", portName);
            return;
        }
        const int baudRate = 9600; //9600 default for arduino
        const Parity parity = Parity.None;

        try
        {
            var serialPort = new SerialPort(foundPortName, baudRate, parity)
            {
                //StopBits = StopBits.None,
                DataBits = 8,
                Handshake = Handshake.None,
                Encoding = Encoding.UTF8
            };

            void OnSerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
            {
                if(serialPort.BytesToRead == 0)
                {
                    return;
                }

                var existingString = serialPort.ReadLine().Replace("\r",string.Empty).Replace("\n",string.Empty);

                RaiseReceivedString(existingString);
                _logger.LogDebug("Got data {portName}:{length} {existingString}", foundPortName, serialPort.BytesToRead, existingString);
            }

            serialPort.DataReceived += OnSerialPortOnDataReceived;

            try
            {
                serialPort.Open();
            }
            catch
            {
                serialPort.DataReceived -= OnSerialPortOnDataReceived;
                serialPort.Dispose();
                throw;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error creating COM port {PortName}",portName);
        }
    } 

    public async Task Close()
    {
        throw new NotImplementedException();
    }

    public event EventHandler<string>? ReceivedString;

    protected void RaiseReceivedString(string message)
    {
        ReceivedString?.Invoke(this, message);
    }
}