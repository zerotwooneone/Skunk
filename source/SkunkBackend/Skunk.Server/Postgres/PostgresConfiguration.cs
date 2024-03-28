using Skunk.Postgres.Interfaces;

namespace Skunk.Server.Postgres;

public class PostgresConfiguration : IPostgresConfig
{
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string Host { get; init; } = "localhost";
    public ulong? Port { get; init; } = 27017;
}