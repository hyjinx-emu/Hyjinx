using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration.Hid
{
    [JsonConverter(typeof(TypedStringEnumConverter<InputBackendType>))]
    public enum InputBackendType
    {
        Invalid,
        WindowKeyboard,
        GamepadSDL2,
    }
}
