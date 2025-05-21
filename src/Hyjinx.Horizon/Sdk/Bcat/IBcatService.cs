using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Bcat;

internal interface IBcatService : IServiceObject
{
    Result RequestSyncDeliveryCache(out IDeliveryCacheProgressService deliveryCacheProgressService);
}