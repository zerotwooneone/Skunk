using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Computer.Domain.Bus.Reactive.Contracts;
using Skunk.Postgres;
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
    private readonly IServiceProvider _serviceProvider;

    public PostgresLifetimeService(
        IPostgresService postgresService,
        IReactiveBus bus,
        ISchedulerLocator schedulerLocator,
        ILogger<PostgresLifetimeService> logger,
        IServiceProvider serviceProvider)
    {
        _disposables = new CompositeDisposable();
        _postgresService = postgresService;
        _bus = bus;
        _logger = logger;
        _serviceProvider = serviceProvider;
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

        result.Add(Subscribe<SensorReading>("sensorRead", OnSensorRead));
        result.Add(Subscribe("PeriodicSensorCheck", OnPeriodicSensorCheck));
            
        return result;
    }

    private async Task OnPeriodicSensorCheck()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var skunkContext = scope.ServiceProvider.GetRequiredService<SkunkContext>();

        IEnumerable<ISensorValue> latestSensorValues;
        try
        {
            latestSensorValues = await _postgresService.GetLatestSensorValues(skunkContext);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error reading Db sensor values");
            return;
        }
        var sensorValues = latestSensorValues as ISensorValue[] ?? latestSensorValues.ToArray();
        if (!sensorValues.Any())
        {
            return;
        }
        var sensorReadings = sensorValues.Select(v=>new SensorReading
        {
            name = v.Type,
            value = v.Value,
            utcTimestamp = v.GetTimestamp()
        }).ToArray();

        await _bus.Publish("LatestSensorValues", (IEnumerable<SensorReading>)sensorReadings);
    }

    private IDisposable Subscribe<T>(string subject, Func<T, Task> handler)
    {
        var subscription = _bus.Subscribe<T>(subject)
            .ObserveOn(_scheduler)
            .Where(o=>o.Param != null)
            .Select(o=>Observable.FromAsync(async ()=> await handler(o.Param!)))
            .Concat()
            .Subscribe();
        return subscription;
    }
    
    private IDisposable Subscribe(string subject, Func<Task> handler)
    {
        var subscription = _bus.Subscribe(subject)
            .ObserveOn(_scheduler)
            .Select(o=>Observable.FromAsync(async ()=> await handler()))
            .Concat()
            .Subscribe();
        
        return subscription;
    }

    private async Task OnSensorRead(SensorReading payload)
    {
        if (string.IsNullOrWhiteSpace(payload.name))
        {
            _logger.LogWarning("Got a sensor payload without a name");
            return;
        }
        
        await using var scope = _serviceProvider.CreateAsyncScope();
        var skunkContext = scope.ServiceProvider.GetRequiredService<SkunkContext>();

        try
        {
            await _postgresService.AddSensorValue(payload.name, payload.value, skunkContext, utcTimestamp: payload.utcTimestamp);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error Adding sensor value");
        }
    }
}