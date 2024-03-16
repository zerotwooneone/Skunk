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

        const string collectionName = "sensors";
        var collection = db.GetCollection<BsonDocument>(collectionName);

        var document = new BsonDocument(new Dictionary<string, object>
        {
            {"type", type},
            {"value", value},
            {"when", (utcTimestamp ?? DateTimeOffset.UtcNow).ToUnixTimeMilliseconds()}
        });
        await collection.InsertOneAsync(document);
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