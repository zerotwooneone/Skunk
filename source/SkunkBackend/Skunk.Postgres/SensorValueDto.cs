using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Skunk.Postgres.Interfaces;

namespace Skunk.Postgres;

public class SensorValueDto: ISensorValue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public long UtcMsTimeStamp { get; init; }
    public float Value { get; init; }
}