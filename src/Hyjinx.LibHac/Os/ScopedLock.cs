using LibHac.Common;
using System.Runtime.CompilerServices;

namespace LibHac.Os;

public static class ScopedLock
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScopedLock<TMutex> Lock<TMutex>(ref TMutex lockable) where TMutex : IBasicLockable
    {
        return new ScopedLock<TMutex>(ref lockable);
    }
}

[NonCopyableDisposable]
public ref struct ScopedLock<TMutex> where TMutex : IBasicLockable
{
    private Ref<TMutex> _mutex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ScopedLock(ref TMutex mutex)
    {
        _mutex = new Ref<TMutex>(ref mutex);
        mutex.Lock();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _mutex.Value.Unlock();
    }
}