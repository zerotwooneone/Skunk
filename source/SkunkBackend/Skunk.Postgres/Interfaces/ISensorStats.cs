namespace Skunk.Postgres.Interfaces;

public interface ISensorStats
{
    string Type { get; }
    float Max { get; }
    float Average { get; }
}