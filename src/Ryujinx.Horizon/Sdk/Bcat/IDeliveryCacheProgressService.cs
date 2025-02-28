using Ryujinx.Horizon.Bcat.Ipc.Types;
using Hyjinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Bcat
{
    internal interface IDeliveryCacheProgressService : IServiceObject
    {
        Result GetEvent(out int handle);
        Result GetImpl(out DeliveryCacheProgressImpl deliveryCacheProgressImpl);
    }
}
