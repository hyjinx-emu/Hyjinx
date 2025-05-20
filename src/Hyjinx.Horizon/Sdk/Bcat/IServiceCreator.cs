using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Ncm;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Bcat
{
    internal interface IServiceCreator : IServiceObject
    {
        Result CreateBcatService(out IBcatService service, ulong pid);
        Result CreateDeliveryCacheStorageService(out IDeliveryCacheStorageService service, ulong pid);
        Result CreateDeliveryCacheStorageServiceWithApplicationId(out IDeliveryCacheStorageService service, ApplicationId applicationId);
    }
}