using Microsoft.Extensions.Options;
using Skunk.Serial.Configuration;
using Skunk.Serial;
using Skunk.Serial.Interfaces;

namespace Skunk.Server.Serial
{
    public class SerialLifetimeService : IHostedService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly SerialConfiguration _serialConfiguration;
        private readonly ILogger<SerialLifetimeService> _logger;
        private IConnection? _connection;

        public SerialLifetimeService(
            IConnectionFactory connectionFactory,
            IOptions<SerialConfiguration> serialConfiguration,
            ILogger<SerialLifetimeService> logger)
        {
            _connectionFactory = connectionFactory;
            _serialConfiguration = serialConfiguration.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //todo: find a working serial port

            if (!_connectionFactory.TryCreate(_serialConfiguration.ComPortName,out _connection) || _connection == null)
            {
                return Task.CompletedTask;
            }
            _connection.ReceivedString += (s, e) => {
                //todo: send data to hub
                _logger.LogInformation("string received {string}", e);
            };

            //todo: implement a method to keep the connection alive
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _connection?.Dispose();
            } catch { 
                //empty
            }

            return Task.CompletedTask;
        }
    }
}
