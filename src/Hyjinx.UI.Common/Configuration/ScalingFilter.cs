using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration
{
    [JsonConverter(typeof(TypedStringEnumConverter<ScalingFilter>))]
    public enum ScalingFilter
    {
        Bilinear,
        Nearest,
        Fsr,
    }
}
