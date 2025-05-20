using System.Text.Json.Serialization;

namespace Hyjinx.HLE.HOS;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AspectRatio
{
    Fixed4x3,
    Fixed16x9,
    Fixed16x10,
    Fixed21x9,
    Fixed32x9,
    Stretched,
}