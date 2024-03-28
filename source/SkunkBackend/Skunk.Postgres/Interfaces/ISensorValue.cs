namespace Skunk.Postgres.Interfaces;

public interface ISensorValue
{
    string Type { get; }
    float Value { get; }
    long UtcMsTimeStamp { get; }

    DateTimeOffset GetTimestamp()
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(UtcMsTimeStamp);
    }
}