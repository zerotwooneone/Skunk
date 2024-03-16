using System.Reactive.Concurrency;

namespace Skunk.Server.Reactive;

public interface ISchedulerLocator
{
    IScheduler GetScheduler(string name);
}