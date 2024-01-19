using System.Text.Json.Serialization;

namespace Skunk.Server.Json
{
    [JsonSerializable(typeof(Complex))]
    public partial class ServerJsonContext : JsonSerializerContext
    {
    }

    /// <summary>
    /// This is just a placeholder because we need a type to serialize otherwise the compiler complains. Remove this once a real [JsonSerializable(typeof())] is added
    /// </summary>
    public record Complex
    {
        public required double Real { get; init; }
        public required double Im { get; init; }
    }
}
