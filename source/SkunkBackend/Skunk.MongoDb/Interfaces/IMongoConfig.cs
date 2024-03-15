namespace Skunk.MongoDb.Interfaces;

public interface IMongoConfig
{
    string? UserName { get; }
    string? Password { get; }
    string Host { get; }
    ulong? Port { get; }
}