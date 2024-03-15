using Skunk.MongoDb.Interfaces;

namespace Skunk.Server.Mongo;

public class MongoConfiguration : IMongoConfig
{
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string Host { get; init; } = "localhost";
    public ulong? Port { get; init; } = 27017;
}