using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AntiAliasing
{
    None,
    Fxaa,
    SmaaLow,
    SmaaMedium,
    SmaaHigh,
    SmaaUltra,
}