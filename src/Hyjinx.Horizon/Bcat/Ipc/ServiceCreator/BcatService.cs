using Hyjinx.Horizon.Bcat.Types;
using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Bcat;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Bcat.Ipc;

partial class BcatService : IBcatService
{
    public BcatService(BcatServicePermissionLevel permissionLevel) { }

    [CmifCommand(10100)]
    public Result RequestSyncDeliveryCache(out IDeliveryCacheProgressService deliveryCacheProgressService)
    {
        deliveryCacheProgressService = new DeliveryCacheProgressService();

        return Result.Success;
    }
}