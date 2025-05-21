#if IS_LEGACY_ENABLED

using LibHac.Diag;
using LibHac.Os.Impl;

namespace LibHac.Os;

public static class SemaphoreApi
{
    public static void InitializeSemaphore(this OsState os, out SemaphoreType semaphore, int initialCount, int maxCount)
    {
        semaphore = new SemaphoreType(os, initialCount, maxCount);
    }

    public static void FinalizeSemaphore(this OsState os, ref SemaphoreType semaphore)
    {
        semaphore.Dispose();
    }

    public static void AcquireSemaphore(this OsState os, ref SemaphoreType semaphore)
    {
        semaphore.Acquire();
    }

    public static bool TryAcquireSemaphore(this OsState os, ref SemaphoreType semaphore)
    {
        return semaphore.TryAcquire();
    }

    public static bool TimedAcquireSemaphore(this OsState os, ref SemaphoreType semaphore, TimeSpan timeout)
    {
        return semaphore.TimedAcquire(timeout);
    }

    public static void ReleaseSemaphore(this OsState os, ref SemaphoreType semaphore)
    {
        semaphore.Release();
    }

    public static void ReleaseSemaphore(this OsState os, ref SemaphoreType semaphore, int count)
    {
        semaphore.Release(count);
    }

    public static int GetCurrentSemaphoreCount(this OsState os, in SemaphoreType semaphore)
    {
        return semaphore.GetCurrentCount();
    }

    public static void InitializeMultiWaitHolder(this OsState os, MultiWaitHolderType holder, Semaphore semaphore)
    {
        Assert.SdkRequires(semaphore.GetBase().CurrentState == SemaphoreType.State.Initialized);

        holder.Impl = new MultiWaitHolderImpl(new MultiWaitHolderOfSemaphore(semaphore));
        holder.UserData = null;
    }
}

#endif