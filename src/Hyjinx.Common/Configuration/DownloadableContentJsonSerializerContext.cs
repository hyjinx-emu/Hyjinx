using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(List<DownloadableContentContainer>))]
    public partial class DownloadableContentJsonSerializerContext : JsonSerializerContext
    {
    }
}
