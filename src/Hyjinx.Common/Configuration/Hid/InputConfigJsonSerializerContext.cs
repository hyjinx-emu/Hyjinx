using Hyjinx.Common.Configuration.Hid.Controller;
using Hyjinx.Common.Configuration.Hid.Keyboard;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration.Hid
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(InputConfig))]
    [JsonSerializable(typeof(StandardKeyboardInputConfig))]
    [JsonSerializable(typeof(StandardControllerInputConfig))]
    public partial class InputConfigJsonSerializerContext : JsonSerializerContext
    {
    }
}
