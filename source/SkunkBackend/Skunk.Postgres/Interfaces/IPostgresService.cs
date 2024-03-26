namespace Skunk.Postgres.Interfaces;

public interface IPostgresService
{
    Task UpdateStartupCount();
    Task AddSensorValue(
        string type, 
        float value,
        SkunkContext skunkContext,
        DateTimeOffset? utcTimestamp = null);

    Task<IEnumerable<SensorValue>> GetLatestSensorValues();
}