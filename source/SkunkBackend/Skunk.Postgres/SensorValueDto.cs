using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skunk.Postgres;

public class SensorValueDto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public long UtcMsTimeStamp { get; init; }
    public float Value { get; init; }
}