using System.Reactive.Concurrency;
using Computer.Domain.Bus.Reactive;
using Computer.Domain.Bus.Reactive.Contracts;

namespace Skunk.Server.DomainBus
{
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddDomainBus(
            this IServiceCollection services)
        {
            return services
                .AddSingleton<IReactiveBus, ReactiveBus>(r=> new ReactiveBus(new EventLoopScheduler()));
        }
    }
}
