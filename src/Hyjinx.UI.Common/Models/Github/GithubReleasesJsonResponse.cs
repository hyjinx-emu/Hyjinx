using System.Collections.Generic;

namespace Hyjinx.UI.Common.Models.Github;

public class GithubReleasesJsonResponse
{
    public string Name { get; set; }
    public List<GithubReleaseAssetJsonResponse> Assets { get; set; }
}