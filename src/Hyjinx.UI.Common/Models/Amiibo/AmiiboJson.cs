using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Models.Amiibo;

public struct AmiiboJson
{
    [JsonPropertyName("amiibo")]
    public List<AmiiboApi> Amiibo { get; set; }
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}