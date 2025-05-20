using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigurationFileFormat))]
    internal partial class ConfigurationJsonSerializerContext : JsonSerializerContext
    {
    }
}