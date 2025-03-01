using System.Text.Json.Serialization;

namespace Hyjinx.UI.Common.Models.Amiibo
{
    [JsonSerializable(typeof(AmiiboJson))]
    public partial class AmiiboJsonSerializerContext : JsonSerializerContext
    {
    }
}
