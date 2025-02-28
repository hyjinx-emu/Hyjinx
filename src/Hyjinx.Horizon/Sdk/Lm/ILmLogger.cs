using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;
using System;

namespace Hyjinx.Horizon.Sdk.Lm
{
    interface ILmLogger : IServiceObject
    {
        Result Log(Span<byte> message);
        Result SetDestination(LogDestination destination);
    }
}
