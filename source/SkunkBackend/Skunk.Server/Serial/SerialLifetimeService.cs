using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Computer.Domain.Bus.Reactive.Contracts;
using Microsoft.Extensions.Options;
using Skunk.Serial.Configuration;
using Skunk.Serial.Interfaces;
using Skunk.Server.DomainBus;

namespace Skunk.Server.Serial
{
    public class SerialLifetimeService : IHostedService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly SerialConfiguration _serialConfiguration;
        private readonly ILogger<SerialLifetimeService> _logger;
        private readonly IReactiveBus _bus;
        private IConnection? _connection;
        private readonly Subject<string> _serialStrings;
        private readonly CompositeDisposable _subscriptions;
        private readonly IScheduler _scheduler;

        public SerialLifetimeService(
            IConnectionFactory connectionFactory,
            IOptions<SerialConfiguration> serialConfiguration,
            ILogger<SerialLifetimeService> logger,
            IReactiveBus bus)
        {
            _connectionFactory = connectionFactory;
            _serialConfiguration = serialConfiguration.Value;
            _logger = logger;
            _bus = bus;

            _serialStrings = new Subject<string>();
            
            _scheduler = new EventLoopScheduler();
            _subscriptions = new CompositeDisposable();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_connection != null)
            {
                throw new InvalidOperationException("The connection already exists");
            }
            //todo: find a working serial port

            if (!TryCreateConnection(_serialConfiguration.ComPortName,out var connection) || connection == null)
            {
                return Task.CompletedTask;
            }

            _connection = connection;
            //todo: implement a method to keep the connection alive
            
            _subscriptions.Add(ThrottleStrings());
            
            return Task.CompletedTask;
        }

        private bool TryCreateConnection(string comPortName, out IConnection? connection)
        {
            if (!_connectionFactory.TryCreate(comPortName,out connection) || connection == null)
            {
                return false;
            }
            
            connection.ReceivedString += OnSerialString;
            return true;
        }

        private IDisposable ThrottleStrings()
        {
            const char separator = ':';
            var asyncObs = _serialStrings
                .AsObservable()
                
                //use a single background thread
                .ObserveOn(_scheduler)
                
                //filter out blank lines
                .Where(s=>!string.IsNullOrWhiteSpace(s))
                
                //separate on character
                .Select(s=>s.Split(separator))
                
                //only where there is exactly one separator and the parts are not blank
                .Where(a=>a.Length ==2 && !string.IsNullOrWhiteSpace(a[0]) && !string.IsNullOrWhiteSpace(a[1]))
                
                //collect these until timespan has elapsed, then send the list
                .Buffer(Observable.Interval(TimeSpan.FromSeconds(1)))
                
                //ignore empty buffers
                .Where(l=>l.Count > 0)
                
                //handle async method
                .Select(a=>Observable.FromAsync(()=>OnThrottledString(a.ToArray()))
                    
                    //probably dont need this, but keep it all on one thread
                    .ObserveOn(_scheduler)
                    
                    //this allows the stream to continue in case of errors, rather than just ending. see below
                    .Materialize())
                
                //one async at a time, in order of arrival 
                .Concat();
            
            var errorHandler = asyncObs
                .Where(n => n.Kind == NotificationKind.OnError)
                .Subscribe(n =>
                {
                    _logger.LogError(n.Exception, "error occurred handling strings");
                });
            
            var success = asyncObs
                .Where(n=>n.Kind == NotificationKind.OnNext)
                
                //need to subscribe to start things
                .Subscribe();
            
            return new CompositeDisposable(success, errorHandler);
        }

        private async Task OnThrottledString(IReadOnlyList<string[]> values)
        {
             var unixms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var value in values)
            {
                if (!float.TryParse(value[1], out var fValue))
                {
                    continue;
                }
                
                var payload = new SensorReading
                {
                    name = value[0],
                    value = fValue,
                    utcUnixMs = unixms
                };
                await _bus.Publish("sensorRead", payload);
            }
        }

        private void OnSerialString(object? sender, string e)
        {
            _serialStrings.OnNext(e);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CleanupConnection();
            
            _subscriptions.Dispose();

            return Task.CompletedTask;
        }

        private void CleanupConnection()
        {
            try
            {
                _connection?.Dispose();
            } catch { 
                //empty
            }
        }
    }
}
