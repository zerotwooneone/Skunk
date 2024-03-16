using Microsoft.Extensions.Options;
using Skunk.MongoDb;
using Skunk.MongoDb.Interfaces;

namespace Skunk.Server.Mongo;

public class MongoLifetimeService : IHostedService
{
    private readonly IMongoConfig _mongoConfig;
    private readonly IMongoService _mongoService;

    public MongoLifetimeService(
        IMongoConfig mongoConfigOptions,
        IMongoService mongoService)
    {
        _mongoConfig = mongoConfigOptions;
        _mongoService = mongoService;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mongoService.UpdateStartupCount();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}