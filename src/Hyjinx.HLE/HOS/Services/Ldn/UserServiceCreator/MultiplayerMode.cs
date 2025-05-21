using System.Text.Json.Serialization;

namespace Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MultiplayerMode
{
    Disabled,
    LdnMitm,
}