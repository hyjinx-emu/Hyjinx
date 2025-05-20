using System;
using System.Threading;

namespace Hyjinx.HLE.HOS.Services;

abstract class DisposableIpcService<T> : IpcService<T>, IDisposable
    where T : DisposableIpcService<T>
{
    private int _disposeState;

    public DisposableIpcService(ServerBase server = null) : base(server) { }

    protected abstract void Dispose(bool isDisposing);

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposeState, 1, 0) == 0)
        {
            Dispose(true);
        }
    }
}