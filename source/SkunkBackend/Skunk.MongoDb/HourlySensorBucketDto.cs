using MongoDB.Bson;

namespace Skunk.MongoDb;

internal class HourlySensorBucketDto
{
    public ObjectId Id { get; init; }
    public long? utcHour { get; init; }
    public string? type { get; init; }
    public HourlySensorValueDto[]? values { get; init; }
}