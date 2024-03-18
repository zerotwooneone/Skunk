namespace Skunk.MongoDb.Interfaces;

public class SensorValue
{
    public string Name { get; init; }
    public float Value { get; init; }
    public long TimeStamp { get; init; }
}