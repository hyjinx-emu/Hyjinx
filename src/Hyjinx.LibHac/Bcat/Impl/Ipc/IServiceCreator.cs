using LibHac.Common;
using System;

namespace LibHac.Bcat.Impl.Ipc;

public interface IServiceCreator : IDisposable
{
    Result CreateDeliveryCacheStorageService(ref SharedRef<IDeliveryCacheStorageService> outService,
        ulong processId);

    Result CreateDeliveryCacheStorageServiceWithApplicationId(
        ref SharedRef<IDeliveryCacheStorageService> outService, ApplicationId applicationId);
}