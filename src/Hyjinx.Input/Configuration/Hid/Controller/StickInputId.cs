using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.Common.Configuration.Hid.Controller
{
    public enum StickInputId : byte
    {
        Unbound,
        Left,
        Right,

        Count,
    }
}