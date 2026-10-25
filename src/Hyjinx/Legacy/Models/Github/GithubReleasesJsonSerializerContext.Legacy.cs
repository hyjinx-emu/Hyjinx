using System.Text.Json.Serialization;

namespace Hyjinx.Ava.Emulation.Models.Github;

[JsonSerializable(typeof(GithubReleasesJsonResponse), GenerationMode = JsonSourceGenerationMode.Metadata)]
public partial class GithubReleasesJsonSerializerContext : JsonSerializerContext
{
}