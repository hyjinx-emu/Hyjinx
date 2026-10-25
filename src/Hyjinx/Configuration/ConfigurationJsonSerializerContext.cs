using Hyjinx.Ava.Configuration;
using System.Text.Json.Serialization;

namespace Hyjinx.Ava.Emulation.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ConfigurationFileFormat))]
internal partial class ConfigurationJsonSerializerContext : JsonSerializerContext
{
}