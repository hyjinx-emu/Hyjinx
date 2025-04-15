using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BackendThreading
{
    Auto,
    Off,
    On,
}
