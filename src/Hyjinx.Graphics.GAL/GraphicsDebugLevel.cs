using System.Text.Json.Serialization;

namespace Hyjinx.Graphics.GAL;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GraphicsDebugLevel
{
    None,
    Error,
    Slowdowns,
    All,
}