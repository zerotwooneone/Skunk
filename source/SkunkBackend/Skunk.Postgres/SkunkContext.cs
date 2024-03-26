using Microsoft.EntityFrameworkCore;
using Skunk.Postgres.Interfaces;

namespace Skunk.Postgres;

public class SkunkContext: DbContext
{
    private readonly IPostgresConfig _config;

    public SkunkContext()
    {
    }
    public SkunkContext(IPostgresConfig config)
    {
        _config = config;
    }
    public DbSet<SensorValueDto> SensorValues { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _config == null
        ? ""
        : GetConnectionString();
        optionsBuilder.UseNpgsql(connectionString:
            connectionString); 
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder builder) 
    {
        builder.UseIdentityColumns();
            //.UseSerialColumns();
    }
    
    private string GetConnectionString()
    {
        var connectionString = $"Server={_config.Host};Port=5432;User Id={_config.UserName};Password={_config.Password};Database=testdb;";
        return connectionString;
    }
}