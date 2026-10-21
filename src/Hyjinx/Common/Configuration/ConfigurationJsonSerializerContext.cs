using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ConfigurationFileFormat))]
internal partial class ConfigurationJsonSerializerContext : JsonSerializerContext
{
}