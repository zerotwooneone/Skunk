namespace Skunk.Server.Hubs
{
    public class SensorPayload
    {
        public Dictionary<string, SensorValues>? Sensors { get; init; }
        public short? Formaldehyde { get; init; }
    }

    public class SensorValues : Dictionary<string, float> {
        public SensorValues(): base() { }
        public SensorValues(Dictionary<string, float> sensorValues) : base(sensorValues) { }
    }
}