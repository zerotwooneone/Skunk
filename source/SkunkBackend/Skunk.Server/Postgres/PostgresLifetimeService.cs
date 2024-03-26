using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Computer.Domain.Bus.Reactive.Contracts;
using Skunk.Postgres.Interfaces;
using Skunk.Server.DomainBus;
using Skunk.Server.Reactive;

namespace Skunk.Server.Postgres;

public class PostgresLifetimeService : IHostedService
{
    private readonly IPostgresService _postgresService;
    private readonly IReactiveBus _bus;
    private readonly CompositeDisposable _disposables;
    private readonly IScheduler _scheduler;
    private readonly ILogger<PostgresLifetimeService> _logger;

    public PostgresLifetimeService(
        IPostgresService postgresService,
        IReactiveBus bus,
        ISchedulerLocator schedulerLocator,
        ILogger<PostgresLifetimeService> logger)
    {
        _disposables = new CompositeDisposable();
        _postgresService = postgresService;
        _bus = bus;
        _logger = logger;
        _scheduler = schedulerLocator.GetScheduler(nameof(PostgresLifetimeService));
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var subscription in Subscribe())
        {
            _disposables.Add(subscription);
        }
        
        await _postgresService.UpdateStartupCount();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _disposables.Dispose();
        return Task.CompletedTask;
    }
    
    private IEnumerable<IDisposable> Subscribe()
    {
        var result = new List<IDisposable>();

        result.Add(Subscribe<SensorReading>("sensorRead", OnSensorRead, "Exception in sensor reading pipeline"));
        result.Add(Subscribe("PeriodicSensorCheck", OnPeriodicSensorCheck, "Exception in sensor check pipeline"));
            
        return result;
    }

    private async Task OnPeriodicSensorCheck()
    {
        var latestSensorValues = await _postgresService.GetLatestSensorValues();
        if (latestSensorValues == null)
        {
            return;
        }

        var sensorValues = latestSensorValues as SensorValue[] ?? latestSensorValues.ToArray();
        if (!sensorValues.Any())
        {
            return;
        }
        var sensorReadings = sensorValues.Select(v=>new SensorReading
        {
            name = v.Name,
            value = v.Value,
            utcUnixMs = v.TimeStamp
        }).ToArray();

        await _bus.Publish("LatestSensorValues", (IEnumerable<SensorReading>)sensorReadings);
    }

    private IDisposable Subscribe<T>(string subject, Func<T, Task> handler, string errorMessage)
    {
        var notificationObs = _bus.Subscribe<T>(subject)
            .ObserveOn(_scheduler)
            .Where(o=>o.Param != null)
            .Select(o=>Observable.FromAsync(async ()=> await handler(o.Param!)))
            .Concat()
            .Materialize();
        
        var errorObs = notificationObs
            .Where(n => n.Kind == NotificationKind.OnError);
        var subscription = errorObs.Subscribe(e =>
        {
            _logger.LogError(e.Exception, errorMessage);
        });
        return subscription;
    }
    
    private IDisposable Subscribe(string subject, Func<Task> handler, string errorMessage)
    {
        var notificationObs = _bus.Subscribe(subject)
            .ObserveOn(_scheduler)
            .Select(o=>Observable.FromAsync(async ()=> await handler()))
            .Concat()
            .Materialize();
        
        var errorObs = notificationObs
            .Where(n => n.Kind == NotificationKind.OnError);
        var subscription = errorObs.Subscribe(e =>
        {
            _logger.LogError(e.Exception, errorMessage);
        });
        return subscription;
    }

    private async Task OnSensorRead(SensorReading payload)
    {
        if (string.IsNullOrWhiteSpace(payload.name))
        {
            _logger.LogWarning("Got a sensor payload without a name");
            return;
        }

        await _postgresService.AddSensorValue(payload.name, payload.value);
    }
}