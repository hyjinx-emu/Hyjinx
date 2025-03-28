using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.Graphics.GAL
{
    [JsonConverter(typeof(TypedStringEnumConverter<GraphicsDebugLevel>))]
    public enum GraphicsDebugLevel
    {
        None,
        Error,
        Slowdowns,
        All,
    }
}
