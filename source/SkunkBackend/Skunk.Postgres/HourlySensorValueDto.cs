namespace Skunk.Postgres;

internal class HourlySensorValueDto
{
    public long? msSinceHour { get; init; }
    public float? value { get; init; }
}