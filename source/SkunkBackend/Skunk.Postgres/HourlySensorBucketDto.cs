namespace Skunk.Postgres;

internal class HourlySensorBucketDto
{
    public ulong Id { get; init; }
    public long? utcHour { get; init; }
    public string? type { get; init; }
    public HourlySensorValueDto[]? values { get; init; }
}