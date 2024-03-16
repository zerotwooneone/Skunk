using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Computer.Domain.Bus.Reactive.Contracts;
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
        foreach (var disposable in Subscribe())
        {
            _disposables.Add(disposable);
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

        var notificationObs = _bus.Subscribe<SensorReading>("sensorRead")
            .ObserveOn(_scheduler)
            .Where(o=>o.Param != null)
            .Select(o=>Observable.FromAsync(async ()=> await OnSensorRead(o.Param!)))
            .Materialize();
        var successObs = notificationObs
            .Where(n => n.Kind == NotificationKind.OnNext)
            .Select(n => n.Value);
        
        var errorObs = notificationObs
            .Where(n => n.Kind == NotificationKind.OnError);
        result.Add(errorObs.Subscribe(e =>
        {
            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "Exception in sensor reading pipeline");
            }
            _logger.LogError("Unknown error in sensor reading pipeline");    
        }));
        
        result.Add(successObs
            .Concat()
            .Subscribe());
            
        return result;
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