using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Logging
{
    [JsonConverter(typeof(TypedStringEnumConverter<LogLevel>))]
    public enum LogLevel
    {
        Debug,
        Stub,
        Info,
        Warning,
        Error,
        Guest,
        AccessLog,
        Notice,
        Trace,
    }
}
