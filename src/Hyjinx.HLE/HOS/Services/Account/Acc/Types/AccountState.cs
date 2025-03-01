using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.HLE.HOS.Services.Account.Acc
{
    [JsonConverter(typeof(TypedStringEnumConverter<AccountState>))]
    public enum AccountState
    {
        Closed,
        Open,
    }
}
