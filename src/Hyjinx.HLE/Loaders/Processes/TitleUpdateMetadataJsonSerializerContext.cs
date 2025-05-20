using System.Text.Json.Serialization;

namespace Hyjinx.HLE.Loaders.Processes;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TitleUpdateMetadata))]
public partial class TitleUpdateMetadataJsonSerializerContext : JsonSerializerContext
{
}