using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Configuration.System;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Region
{
    Japan,
    USA,
    Europe,
    Australia,
    China,
    Korea,
    Taiwan,
}
