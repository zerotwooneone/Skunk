using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Computer.Domain.Bus.Reactive.Contracts;
using Skunk.Postgres.Interfaces;
using Skunk.Server.DomainBus;
using Skunk.Server.Reactive;

namespace Skunk.Server.Hubs;

public class FrontEndLifetimeService : IHostedService
{
    private readonly IReactiveBus _bus;
    private readonly CompositeDisposable _disposables;
    private readonly IScheduler _scheduler;
    private readonly ILogger<FrontEndLifetimeService> _logger;
    private readonly IFrontendService _frontendService;

    public FrontEndLifetimeService(
        IReactiveBus bus,
        ISchedulerLocator schedulerLocator,
        ILogger<FrontEndLifetimeService> logger,
        IFrontendService frontendService)
    {
        _disposables = new CompositeDisposable();
        _bus = bus;
        _logger = logger;
        _frontendService = frontendService;
        _scheduler = schedulerLocator.GetScheduler(nameof(FrontEndLifetimeService));
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var subscription in Subscribe())
        {
            _disposables.Add(subscription);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _disposables.Dispose();
        return Task.CompletedTask;
    }
    
    private IEnumerable<IDisposable> Subscribe()
    {
        var result = new List<IDisposable>();

        var appStartupGracePeriod = TimeSpan.FromSeconds(2);
        
        var updateSubscription = GetUpdateSubscription(appStartupGracePeriod);
        result.Add(updateSubscription);
        
        var statsSubscription = PeriodicStatsRequestSubscription(appStartupGracePeriod);
        result.Add(statsSubscription);

        var latestValueSubscription = LatestValueChangedSubscription();
        result.Add(latestValueSubscription);
        
        result.Add(LatestSensorStatsSubscription());
        
        return result;
    }

    private IDisposable LatestSensorStatsSubscription()
    {
        var currentObs = _bus
            .Subscribe<IEnumerable<ISensorStats>>("LatestSensorStats")
            .ObserveOn(_scheduler)
            .Where(b=>b.Param != null)
            .Select(b=>Observable.FromAsync(async ()=>await OnNewSensorStats(b.Param!)))
            .Concat()
            .Materialize();
        
        var currentError = currentObs
            .Where(o => o.Kind == NotificationKind.OnError);

        var latestValueSubscription = currentError.Subscribe(e =>
        {
            _logger.LogError(e.Exception, "Got an error in stats pipeline");
        });
        return latestValueSubscription;
    }

    private async Task OnNewSensorStats(IEnumerable<ISensorStats> stats)
    {
        await _frontendService.SendSensorStats(stats);
    }

    private IDisposable LatestValueChangedSubscription()
    {
        var currentObs = _bus
            .Subscribe<IEnumerable<SensorReading>>("LatestSensorValues")
            .ObserveOn(_scheduler)
            .Where(b=>b.Param != null)
            .Select(b=>Observable.FromAsync(async ()=>await OnNewSensorValues(b.Param!)))
            .Concat()
            .Materialize();
        
        var currentError = currentObs
            .Where(o => o.Kind == NotificationKind.OnError);

        var latestValueSubscription = currentError.Subscribe(e =>
        {
            _logger.LogError(e.Exception, "Got an error in current value pipeline");
        });
        return latestValueSubscription;
    }

    private IDisposable GetUpdateSubscription(TimeSpan appStartupGracePeriod)
    {
        var updateInterval = TimeSpan.FromSeconds(1);
        var updateObs =
            Observable
                .Return(Unit.Default)
                .Delay(appStartupGracePeriod, _scheduler)
                .ObserveOn(_scheduler)
                .Select(_=>Observable.Interval(updateInterval))
                .Switch()
                .Select(_=>Observable.FromAsync(OnUpdate) )
                .Concat()
                .Materialize();

        var updateError = updateObs
            .Where(o => o.Kind == NotificationKind.OnError);

        var updateSubscription = updateError.Subscribe(e =>
        {
            _logger.LogError(e.Exception, "Got an error in update pipeline");
        });
        return updateSubscription;
    }
    
    private IDisposable PeriodicStatsRequestSubscription(TimeSpan appStartupGracePeriod)
    {
        var updateInterval = TimeSpan.FromMinutes(10);
        var updateObs =
            Observable
                .Return(Unit.Default)
                .Delay(appStartupGracePeriod, _scheduler)
                .ObserveOn(_scheduler)
                .Select(_=>Observable.Return(Unit.Default).Merge(Observable.Interval(updateInterval).Select(_=>Unit.Default)))
                .Switch()
                .Select(_=>Observable.FromAsync(OnUpdateStats) )
                .Concat()
                .Materialize();

        var updateError = updateObs
            .Where(o => o.Kind == NotificationKind.OnError);

        var updateSubscription = updateError.Subscribe(e =>
        {
            _logger.LogError(e.Exception, "Got an error in update pipeline");
        });
        return updateSubscription;
    }

    private async Task OnNewSensorValues(IEnumerable<SensorReading> values)
    {
        var sensorReadings = values as SensorReading[] ?? values.ToArray();
        if (!sensorReadings.Any())
        {
            return;
        }
        const string valueKey = "value";
        Dictionary<string,SensorValues> dict = new Dictionary<string, SensorValues>();
        foreach (var value in sensorReadings)
        {
            if (string.IsNullOrWhiteSpace(value.name))
            {
                continue;
            }
            dict[value.name] = new SensorValues
            {
                {valueKey, value.value}
            };
        }
            
        var sensorPayload = new SensorPayload()
        {
            Sensors = dict,
        };
        await _frontendService.SendSensorPayload(sensorPayload);
    }

    private async Task OnUpdate()
    {
        await _bus.Publish("PeriodicSensorCheck");
    }
    
    private async Task OnUpdateStats()
    {
        await _bus.Publish("StatsUpdateRequest");
    }
}