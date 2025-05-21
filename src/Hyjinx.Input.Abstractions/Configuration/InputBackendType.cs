using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration.Hid;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputBackendType
{
    Invalid,
    WindowKeyboard,
    GamepadSDL2,
}