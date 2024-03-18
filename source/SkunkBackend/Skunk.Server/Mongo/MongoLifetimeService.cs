using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Computer.Domain.Bus.Reactive.Contracts;
using MongoDB.Bson.Serialization.Conventions;
using Skunk.MongoDb.Interfaces;
using Skunk.Server.DomainBus;
using Skunk.Server.Reactive;

namespace Skunk.Server.Mongo;

public class MongoLifetimeService : IHostedService
{
    private readonly IMongoService _mongoService;
    private readonly IReactiveBus _bus;
    private readonly CompositeDisposable _disposables;
    private readonly IScheduler _scheduler;
    private readonly ILogger<MongoLifetimeService> _logger;

    public MongoLifetimeService(
        IMongoService mongoService,
        IReactiveBus bus,
        ISchedulerLocator schedulerLocator,
        ILogger<MongoLifetimeService> logger)
    {
        _disposables = new CompositeDisposable();
        _mongoService = mongoService;
        _bus = bus;
        _logger = logger;
        _scheduler = schedulerLocator.GetScheduler(nameof(MongoLifetimeService));
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Allows automapping of the camelCase database fields to models 
        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
        
        foreach (var subscription in Subscribe())
        {
            _disposables.Add(subscription);
        }
        
        await _mongoService.UpdateStartupCount();
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
        var latestSensorValues = await _mongoService.GetLatestSensorValues();
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

        await _mongoService.AddSensorValue(payload.name, payload.value);
    }
}