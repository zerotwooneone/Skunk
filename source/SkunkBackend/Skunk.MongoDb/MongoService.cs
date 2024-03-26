using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Skunk.MongoDb.Interfaces;

namespace Skunk.MongoDb;

public class MongoService : IMongoService
{
    private readonly IMongoConfig _config;
    private readonly ILogger<MongoService> _logger;
    private readonly Lazy<MongoClient> _client;
    private readonly Lazy<IMongoDatabase> _database;
    private readonly Lazy<IMongoCollection<HourlySensorBucketDto>> _hourlyCollection;
    const string DbName="skunk";

    public MongoService(
        IMongoConfig config,
        ILogger<MongoService> logger)
    {
        _config = config ?? throw new ArgumentException("config cannot be null");
        _logger = logger;
        _client = new Lazy<MongoClient>(() => new MongoClient(GetConnectionString()));
        _database = new Lazy<IMongoDatabase>(() =>
        {
            var db = _client.Value.GetDatabase(DbName);

            if (db == null)
            {
                _logger.LogWarning("Database was null name:{DbName}", DbName);
            }

            return db;
        });
        _hourlyCollection = new Lazy<IMongoCollection<HourlySensorBucketDto>>(() =>
            _database.Value.GetCollection<HourlySensorBucketDto>("hourlySensors"));
    }
    public async Task UpdateStartupCount()
    {
        
        var db = _client.Value.GetDatabase(DbName);

        if (db == null)
        {
            _logger.LogWarning("Database was null name:{DbName}", DbName);
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
        var utcUnixMs = (utcTimestamp ?? DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();

        const long millisecondsPerHour=60*60*1000;
        
        //this truncates the non-hour milliseconds from the value;
        var utcHour = (utcUnixMs / millisecondsPerHour) * millisecondsPerHour;
        
        var updateFilter = new ObjectFilterDefinition<BsonDocument>(new BsonDocument
        {
            {"type", type},
            {"utcHour", utcHour}
        });

        var msSinceHour = utcUnixMs % millisecondsPerHour;
        var update = Builders<BsonDocument>.Update.Push("values", new BsonDocument()
        {
            {"value", value},
            {"msSinceHour", msSinceHour}
        });
        
        var updateOptions = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true
        };
        await _database.Value.GetCollection<BsonDocument>("hourlySensors").FindOneAndUpdateAsync(updateFilter, update, updateOptions);
    }

    public async Task<IEnumerable<SensorValue>> GetLatestSensorValues()
    {
        var document = Queryable.OrderByDescending(_hourlyCollection.Value
                .AsQueryable(), b=>b.utcHour)
            .FirstOrDefault();

        if (document == null)
        {
            return Enumerable.Empty<SensorValue>();    
        }
        
        var targetHour = document.utcHour;
        

        var allSensors = await _hourlyCollection.Value
            .AsQueryable()
            .Where(b => b.utcHour == targetHour)
            .ToListAsync();

        if (allSensors == null)
        {
            return Enumerable.Empty<SensorValue>();
        }
        
        return allSensors.SelectMany(bucketDto =>
        {
            if (bucketDto.values == null ||
                string.IsNullOrWhiteSpace(bucketDto.type) ||
                bucketDto.utcHour == null)
            {
                return Array.Empty<SensorValue>();
            }
            //todo: should sort, but for now we just guess that the last one is the right one
            var lastValue = bucketDto.values.Last();
            if (lastValue.msSinceHour == null ||
                lastValue.value == null)
            {
                return Array.Empty<SensorValue>();
            }
            return new[]{ 
                new SensorValue
                {
                    Name = bucketDto.type,
                    TimeStamp = bucketDto.utcHour.Value + lastValue.msSinceHour.Value,
                    Value = lastValue.value.Value
                }
            };
        }).ToArray();
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