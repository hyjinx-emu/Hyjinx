using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hyjinx.Ava.Emulation;

[JsonSerializable(typeof(string[]), TypeInfoPropertyName = "StringArray")]
[JsonSerializable(typeof(Dictionary<string, string>), TypeInfoPropertyName = "StringDictionary")]
internal partial class CommonJsonContext : JsonSerializerContext
{
}