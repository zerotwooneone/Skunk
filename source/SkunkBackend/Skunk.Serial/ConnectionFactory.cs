using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skunk.Serial.Configuration;
using Skunk.Serial.Interfaces;

namespace Skunk.Serial;

public class ConnectionFactory : IConnectionFactory
{
    private readonly ILogger<ConnectionFactory> _logger;
    private readonly ILogger<Connection> _connectionLogger;
    private readonly SerialConfiguration _configuration;

    public ConnectionFactory(
        ILogger<ConnectionFactory> logger,
        IOptions<SerialConfiguration> configuration,
        ILogger<Connection> connectionLogger)
    {
        _logger = logger;
        this._connectionLogger = connectionLogger;
        _configuration = configuration.Value;
    }
    public IEnumerable<string> GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    public bool TryCreate(string portName, out IConnection? connection)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            _logger.LogError("Portname was null or blank");
            connection = null;
            return false;
        }
        var portNames = GetPortNames();
        _logger.LogDebug("Found port names {names}", string.Join(",", portNames));

        var foundPortName = portNames.FirstOrDefault(p => p.Equals(portName, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(foundPortName))
        {
            _logger.LogError("Portname was not found {portName}", portName);
            connection = null;
            return false;
        }
        const Parity parity = Parity.None;

        try
        {
            var serialPort = new SerialPort(foundPortName, _configuration.BaudRate, parity)
            {
                //StopBits = StopBits.None,
                DataBits = 8,
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII
            };

            try
            {
                connection = new Connection(_connectionLogger, serialPort);
                serialPort.Open();
                serialPort.DtrEnable = true;
                _logger.LogInformation("Opened serial port {portName}", serialPort.PortName);
                return true;
            }
            catch
            {
                serialPort.Dispose();
                throw;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating COM port {PortName}", foundPortName);
        }
        connection = null;
        return false;
    }
}