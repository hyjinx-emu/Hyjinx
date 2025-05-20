using LibHac.Common;
using System;

namespace LibHac.Bcat.Impl.Ipc;

public interface IDeliveryCacheStorageService : IDisposable
{
    Result CreateFileService(ref SharedRef<IDeliveryCacheFileService> outFileService);
    Result CreateDirectoryService(ref SharedRef<IDeliveryCacheDirectoryService> outDirectoryService);
    Result EnumerateDeliveryCacheDirectory(out int namesRead, Span<DirectoryName> nameBuffer);
}