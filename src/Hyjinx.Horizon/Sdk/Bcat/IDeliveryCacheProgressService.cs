using Hyjinx.Horizon.Bcat.Ipc.Types;
using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Bcat;

internal interface IDeliveryCacheProgressService : IServiceObject
{
    Result GetEvent(out int handle);
    Result GetImpl(out DeliveryCacheProgressImpl deliveryCacheProgressImpl);
}