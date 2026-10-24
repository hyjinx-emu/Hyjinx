using System.Text.Json.Serialization;

namespace Hyjinx.Ava.Emulation.Models.Amiibo;

[JsonSerializable(typeof(AmiiboJson))]
public partial class AmiiboJsonSerializerContext : JsonSerializerContext
{
}