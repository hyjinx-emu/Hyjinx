using System.Text.Json.Serialization;

namespace Hyjinx.Ava.Emulation.Library;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ApplicationMetadata))]
internal partial class ApplicationJsonSerializerContext : JsonSerializerContext
{
}