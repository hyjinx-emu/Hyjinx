using System.Text.Json.Serialization;

namespace Hyjinx.Common.Logging
{
    [JsonSerializable(typeof(LogEventArgsJson))]
    internal partial class LogEventJsonSerializerContext : JsonSerializerContext
    {
    }
}
