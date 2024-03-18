namespace Skunk.MongoDb.Interfaces;

public interface IMongoService
{
    Task UpdateStartupCount();
    Task AddSensorValue(
        string type, 
        float value, 
        DateTimeOffset? utcTimestamp = null);

    Task<IEnumerable<SensorValue>> GetLatestSensorValues();
}