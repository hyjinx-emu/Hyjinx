using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;
using LibHac.Bcat;
using System;

namespace Hyjinx.Horizon.Sdk.Bcat;

internal interface IDeliveryCacheStorageService : IServiceObject
{
    Result CreateDirectoryService(out IDeliveryCacheDirectoryService service);
    Result CreateFileService(out IDeliveryCacheFileService service);
    Result EnumerateDeliveryCacheDirectory(out int count, Span<DirectoryName> directoryNames);
}