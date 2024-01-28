
using Skunk.Serial;
using Skunk.Serial.Configuration;
using Skunk.Serial.Interfaces;

namespace Skunk.Server.Serial
{
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddSerial(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .AddSingleton<IConnectionFactory, ConnectionFactory>()
                .Configure<SerialConfiguration>(configuration.GetSection("Serial"))
                .AddHostedService<SerialLifetimeService>();
        }
    }
}
