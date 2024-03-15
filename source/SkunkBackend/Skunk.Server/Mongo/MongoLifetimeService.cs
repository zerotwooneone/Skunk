using Microsoft.Extensions.Options;
using Skunk.MongoDb;
using Skunk.MongoDb.Interfaces;

namespace Skunk.Server.Mongo;

public class MongoLifetimeService : IHostedService
{
    private readonly IMongoConfig _mongoConfig;

    public MongoLifetimeService(IMongoConfig mongoConfigOptions)
    {
        _mongoConfig = mongoConfigOptions;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var service = new MongoService(_mongoConfig);
        var o = service.Find();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}