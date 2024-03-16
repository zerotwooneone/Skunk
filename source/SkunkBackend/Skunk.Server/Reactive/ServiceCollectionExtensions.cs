namespace Skunk.Server.Reactive
{
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddReactive(
            this IServiceCollection services)
        {
            return services
                .AddSingleton<ISchedulerLocator, SchedulerLocator>();
        }
    }
}
