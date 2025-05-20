using System.Text.Json.Serialization;

namespace Hyjinx.HLE.Loaders.Processes;

public struct DownloadableContentNca
{
    [JsonPropertyName("path")]
    public string FullPath { get; set; }
    [JsonPropertyName("title_id")]
    public ulong TitleId { get; set; }
    [JsonPropertyName("is_enabled")]
    public bool Enabled { get; set; }
}