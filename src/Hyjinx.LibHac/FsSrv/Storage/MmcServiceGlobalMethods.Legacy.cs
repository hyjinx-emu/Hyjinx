#if IS_LEGACY_ENABLED

namespace LibHac.FsSrv.Storage;

/// <summary>
/// Contains global MMC-storage-related functions.
/// </summary>
/// <remarks>Based on nnSdk 16.2.0 (FS 16.0.0)</remarks>
public static class MmcServiceGlobalMethods
{
    public static Result GetAndClearPatrolReadAllocateBufferCount(this FileSystemServer fsSrv, out long successCount,
        out long failureCount)
    {
        return fsSrv.Storage.GetAndClearPatrolReadAllocateBufferCount(out successCount, out failureCount);
    }
}

#endif
