

namespace Skunk.Server.Hubs
{
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddHubs(
            this IServiceCollection services)
        {
            return services
                .AddSingleton<IClientService, ClientService>();
        }
    }
}
