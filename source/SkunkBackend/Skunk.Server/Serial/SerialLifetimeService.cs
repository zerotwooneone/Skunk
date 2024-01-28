using Microsoft.Extensions.Options;
using Skunk.Serial.Configuration;
using Skunk.Serial.Interfaces;
using Skunk.Server.Hubs;

namespace Skunk.Server.Serial
{
    public class SerialLifetimeService : IHostedService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly SerialConfiguration _serialConfiguration;
        private readonly ILogger<SerialLifetimeService> _logger;
        private readonly IClientService _clientService;
        private IConnection? _connection;

        public SerialLifetimeService(
            IConnectionFactory connectionFactory,
            IOptions<SerialConfiguration> serialConfiguration,
            ILogger<SerialLifetimeService> logger,
            IClientService clientService)
        {
            _connectionFactory = connectionFactory;
            _serialConfiguration = serialConfiguration.Value;
            _logger = logger;
            _clientService = clientService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //todo: find a working serial port

            if (!_connectionFactory.TryCreate(_serialConfiguration.ComPortName,out _connection) || _connection == null)
            {
                return Task.CompletedTask;
            }

            _connection.ReceivedString += OnSerialString;

            //todo: implement a method to keep the connection alive
            
            return Task.CompletedTask;
        }

        private async void OnSerialString(object? sender, string e)
        {
            _logger.LogInformation("string received {String}", e);
            const string BzHeader = "BZ:";
            if (!e.StartsWith(BzHeader))
            {
                return;
            }

            var textValue = e.Replace(BzHeader, string.Empty);
            if (!short.TryParse(textValue, out var bzValue))
            {
                _logger.LogError("BZ value could not be parsed {Message}", e);
                return;
            }

            await _clientService.SendFormaldehydeValue(bzValue);
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
