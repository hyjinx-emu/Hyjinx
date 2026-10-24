using System.Collections.Generic;

namespace Hyjinx.Ava.Emulation.Models.Github;

public class GithubReleasesJsonResponse
{
    public string Name { get; set; }
    public List<GithubReleaseAssetJsonResponse> Assets { get; set; }
}