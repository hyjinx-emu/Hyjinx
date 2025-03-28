using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration
{
    [JsonConverter(typeof(TypedStringEnumConverter<AntiAliasing>))]
    public enum AntiAliasing
    {
        None,
        Fxaa,
        SmaaLow,
        SmaaMedium,
        SmaaHigh,
        SmaaUltra,
    }
}
