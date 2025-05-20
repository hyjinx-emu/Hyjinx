using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hyjinx.HLE.Loaders.Processes
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(List<DownloadableContentContainer>))]
    public partial class DownloadableContentJsonSerializerContext : JsonSerializerContext
    {
    }
}