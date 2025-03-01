using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration
{
    [JsonConverter(typeof(TypedStringEnumConverter<GraphicsBackend>))]
    public enum GraphicsBackend
    {
        Vulkan,
        OpenGl,
    }
}
