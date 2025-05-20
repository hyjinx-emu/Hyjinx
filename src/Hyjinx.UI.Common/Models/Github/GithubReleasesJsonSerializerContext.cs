using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Models.Github
{
    [JsonSerializable(typeof(GithubReleasesJsonResponse), GenerationMode = JsonSourceGenerationMode.Metadata)]
    public partial class GithubReleasesJsonSerializerContext : JsonSerializerContext
    {
    }
}