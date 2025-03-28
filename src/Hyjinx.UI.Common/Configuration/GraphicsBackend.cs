using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration
{
    [JsonConverter(typeof(TypedStringEnumConverter<GraphicsBackend>))]
    public enum GraphicsBackend
    {
        Vulkan,
        OpenGl,
    }
}
