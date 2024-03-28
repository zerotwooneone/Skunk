using System.Reactive.Concurrency;

namespace Skunk.Server.Reactive;

public class SchedulerLocator : ISchedulerLocator
{
    public IScheduler GetScheduler(string name)
    {
        //todo:consider caching these
        return new EventLoopScheduler();
    }
}