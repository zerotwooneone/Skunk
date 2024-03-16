using MongoDB.Bson;
using MongoDB.Driver;
using Skunk.MongoDb.Interfaces;

namespace Skunk.MongoDb;

public class MongoService : IMongoService
{
    private readonly IMongoConfig _config;
    private readonly Lazy<MongoClient> _client;
    const string DbName="skunk";

    public MongoService(IMongoConfig config)
    {
        _config = config ?? throw new ArgumentException("config cannot be null");
        _client = new Lazy<MongoClient>(() => new MongoClient(GetConnectionString()));
    }
    public async Task UpdateStartupCount()
    {
        
        var db = _client.Value.GetDatabase(DbName);

        if (db == null)
        {
            return;
        }

        const string collectionName = "application";
        var collection = db.GetCollection<BsonDocument>(collectionName);

        var filter = Builders<BsonDocument>.Filter.Eq("type", "startupStats");

        var updateOptions = new FindOneAndUpdateOptions<BsonDocument>{IsUpsert = true};
        
        var update = Builders<BsonDocument>.Update
            .Inc("startupCount", 1)
            .Set("lastStartUnixUtcMs", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var document = await collection.FindOneAndUpdateAsync(
            filter,
            update,
            updateOptions);
    }

    public async Task AddSensorValue(string type, float value, DateTimeOffset? utcTimestamp = null)
    {
        var db = _client.Value.GetDatabase(DbName);

        if (db == null)
        {
            return;
        }

        var utcUnixMs = (utcTimestamp ?? DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();

        const long millisecondsPerHour=60*60*1000;
        
        //this truncates the non-hour milliseconds from the value;
        var utcHour = (utcUnixMs / millisecondsPerHour) * millisecondsPerHour;
        
        var updateFilter = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
        {
            {"type", type},
            {"utcHour", utcHour}
        });

        var msSinceHour = utcUnixMs % millisecondsPerHour;
        var update = Builders<BsonDocument>.Update.Push("values", new BsonDocument
        {
            {"value", value},
            {"msSinceHour", msSinceHour}
        });
        var updateOptions = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true
        };
        await db.GetCollection<BsonDocument>("hourlySensors").FindOneAndUpdateAsync(updateFilter, update, updateOptions);
    }

    private string GetConnectionString()
    {
        var authPart = string.IsNullOrWhiteSpace(_config.UserName) || string.IsNullOrWhiteSpace(_config.Password)
            ? string.Empty
            : $"{_config.UserName}:{_config.Password}@";
        var portPart = _config.Port is null or < 1
            ? string.Empty
            : $":{_config.Port}";
        var connectionString = $"mongodb://{authPart}{_config.Host}{portPart}";
        return connectionString;
    }
}