using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.LogManager.Ipc;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Lm;

interface ILogService : IServiceObject
{
    Result OpenLogger(out LmLogger logger, ulong pid);
}