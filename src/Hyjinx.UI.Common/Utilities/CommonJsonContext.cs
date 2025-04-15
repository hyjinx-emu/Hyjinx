using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Utilities
{
    [JsonSerializable(typeof(string[]), TypeInfoPropertyName = "StringArray")]
    [JsonSerializable(typeof(Dictionary<string, string>), TypeInfoPropertyName = "StringDictionary")]
    public partial class CommonJsonContext : JsonSerializerContext
    {
    }
}
