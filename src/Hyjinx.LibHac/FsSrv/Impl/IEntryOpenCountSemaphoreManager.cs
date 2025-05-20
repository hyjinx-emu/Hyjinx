using LibHac.Common;
using LibHac.FsSystem;
using System;

namespace LibHac.FsSrv.Impl;

public interface IEntryOpenCountSemaphoreManager : IDisposable
{
    Result TryAcquireEntryOpenCountSemaphore(ref UniqueRef<IUniqueLock> outSemaphore);
}