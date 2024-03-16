namespace Skunk.Server.DomainBus;

public class SensorReading
{
    public string name { get; init; }
    public float value { get; init; }
    public long utcUnixMs { get; init; }
}