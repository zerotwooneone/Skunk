using Microsoft.Extensions.Options;
using Skunk.MongoDb.Interfaces;

namespace Skunk.Server.Mongo
{
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddMongo(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .Configure<MongoConfiguration>(configuration.GetSection("mongo"))
                .AddSingleton<IMongoConfig>(r=>
                {
                    var c = r.GetService<IOptions<MongoConfiguration>>()?.Value;
                    if (c == null)
                    {
                        //todo:log this
                        return new MongoConfiguration();
                    }
                    return c;
                })
                .AddHostedService<MongoLifetimeService>();
        }
    }
}
