using LibHac.Common;
using System;
using ApplicationId = LibHac.Ncm.ApplicationId;

namespace LibHac.Bcat.Impl.Ipc;

public interface IServiceCreator : IDisposable
{
    Result CreateDeliveryCacheStorageService(ref SharedRef<IDeliveryCacheStorageService> outService,
        ulong processId);

    Result CreateDeliveryCacheStorageServiceWithApplicationId(
        ref SharedRef<IDeliveryCacheStorageService> outService, ApplicationId applicationId);
}