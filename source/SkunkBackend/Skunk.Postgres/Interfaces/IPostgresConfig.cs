namespace Skunk.Postgres.Interfaces;

public interface IPostgresConfig
{
    string? UserName { get; }
    string? Password { get; }
    string Host { get; }
    ulong? Port { get; }
}