using Microsoft.Extensions.Options;
using Skunk.Postgres;
using Skunk.Postgres.Interfaces;

namespace Skunk.Server.Postgres
{
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddPostgres(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .Configure<PostgresConfiguration>(configuration.GetSection("postgres"))
                .AddSingleton<IPostgresConfig>(r=>
                {
                    var c = r.GetService<IOptions<PostgresConfiguration>>()?.Value;
                    if (c == null)
                    {
                        //todo:log this
                        return new PostgresConfiguration();
                    }
                    return c;
                })
                .AddSingleton<IPostgresService, PostgresService>()
                .AddHostedService<PostgresLifetimeService>();
        }
    }
}
