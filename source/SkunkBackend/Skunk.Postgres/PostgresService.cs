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

    public async Task AddSensorValue(
        string type, 
        float value,
        SkunkContext skunkContext,
        DateTimeOffset? utcTimestamp = null)
    {
        var utcUnixMs = (utcTimestamp ?? DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();
        await skunkContext.SensorValues.AddAsync(new SensorValueDto
        {
            Type = type,
            Value = value,
            UtcMsTimeStamp = utcUnixMs
        });

        await skunkContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ISensorValue>> GetLatestSensorValues(SkunkContext skunkContext)
    {
        const int maxSanityLimit = 1000;
        var x = skunkContext.SensorValues
            .GroupBy(s => s.Type)
            .Select(typeGroup => typeGroup.OrderByDescending(g => g.UtcMsTimeStamp).First())
            .Take(maxSanityLimit)
            .ToArray();
        return x;
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