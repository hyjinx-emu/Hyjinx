using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration.Hid.Controller.Motion
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(MotionConfigController))]
    [JsonSerializable(typeof(CemuHookMotionConfigController))]
    [JsonSerializable(typeof(StandardMotionConfigController))]
    public partial class MotionConfigJsonSerializerContext : JsonSerializerContext
    {
    }
}
