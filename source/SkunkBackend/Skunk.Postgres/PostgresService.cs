using Microsoft.Extensions.Logging;
using Npgsql;
using Skunk.Postgres.Interfaces;

namespace Skunk.Postgres;

public class PostgresService : IPostgresService
{
    private readonly IPostgresConfig _config;
    private readonly ILogger<PostgresService> _logger;
    const string DbName="skunk";

    private readonly Lazy<NpgsqlConnection> _connection;

    public PostgresService(
        IPostgresConfig config,
        ILogger<PostgresService> logger)
    {
        _config = config ?? throw new ArgumentException("config cannot be null");
        _logger = logger;
        
        _connection = new Lazy<NpgsqlConnection>(() =>
        {
            var con = new NpgsqlConnection(
                connectionString: GetConnectionString());
            con.Open();
            return con;
        });
    }

    public async Task UpdateStartupCount()
    {
        //throw new NotImplementedException();
    }

    public async Task AddSensorValue(string type, float value, DateTimeOffset? utcTimestamp = null)
    {
        var command = GetCommand();
        /*var utcUnixMs = (utcTimestamp ?? DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();

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
        await _database.Value.GetCollection<BsonDocument>("hourlySensors").FindOneAndUpdateAsync(updateFilter, update, updateOptions);*/
    }

    public async Task<IEnumerable<SensorValue>> GetLatestSensorValues()
    {
        /*var document = Queryable.OrderByDescending(_hourlyCollection.Value
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
        }).ToArray();*/
        return Enumerable.Empty<SensorValue>();
    }
    
    private NpgsqlCommand GetCommand()
    {
        return new NpgsqlCommand
        {
            Connection = _connection.Value
        };
    }

    private string GetConnectionString()
    {
        var connectionString = $"Server={_config.Host};Port=5432;User Id={_config.UserName};Password={_config.Password};Database=testdb;";
        return connectionString;
    }
}