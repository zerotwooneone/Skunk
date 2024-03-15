using MongoDB.Bson;
using MongoDB.Driver;
using Skunk.MongoDb.Interfaces;

namespace Skunk.MongoDb;

public class MongoService
{
    private readonly IMongoConfig _config;

    public MongoService(IMongoConfig config)
    {
        _config = config ?? throw new ArgumentException("config cannot be null");
    }
    public object? Find()
    {
        var authPart = string.IsNullOrWhiteSpace(_config.UserName) || string.IsNullOrWhiteSpace(_config.Password)
            ? string.Empty
            : $"{_config.UserName}:{_config.Password}@";
        var portPart = _config.Port is null or < 1
            ? string.Empty
            : $":{_config.Port}";
        var connectionString = $"mongodb://{authPart}{_config.Host}{portPart}";

        var driver = new MongoClient(connectionString);
        
        const string dbName="skunk";
        var db = driver.GetDatabase(dbName);

        if (db == null)
        {
            return null;
        }

        const string collectionName = "test";
        var collection = db.GetCollection<BsonDocument>(collectionName);

        var filter = Builders<BsonDocument>.Filter.Eq("propName", "prop value");

        var document = collection.Find(filter).FirstOrDefault();

        return document;
    }
}