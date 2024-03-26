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
        //todo: fix this dirty hack to let migrations work. for now, put static migration string here
        var connectionString = _config == null
        ? ""
        : GetConnectionString();
        optionsBuilder.UseNpgsql(connectionString:
            connectionString); 
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder builder) 
    {
        //this allows columns to autoincrement 
        builder.UseIdentityColumns();
    }
    
    private string GetConnectionString()
    {
        var connectionString = $"Server={_config.Host};Port=5432;User Id={_config.UserName};Password={_config.Password};Database=testdb;";
        return connectionString;
    }
}