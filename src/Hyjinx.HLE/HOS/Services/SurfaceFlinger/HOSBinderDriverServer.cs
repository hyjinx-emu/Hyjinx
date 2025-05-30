using Hyjinx.HLE.HOS.Kernel.Threading;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Hyjinx.HLE.HOS.Services.SurfaceFlinger;

partial class HOSBinderDriverServer : IHOSBinderDriver
{
    private static readonly Dictionary<int, IBinder> _registeredBinderObjects = new();

    private static int _lastBinderId = 0;

    private static readonly object _lock = new();

    public static int RegisterBinderObject(IBinder binder)
    {
        lock (_lock)
        {
            _lastBinderId++;

            _registeredBinderObjects.Add(_lastBinderId, binder);

            return _lastBinderId;
        }
    }

    public static void UnregisterBinderObject(int binderId)
    {
        lock (_lock)
        {
            _registeredBinderObjects.Remove(binderId);
        }
    }

    public static int GetBinderId(IBinder binder)
    {
        lock (_lock)
        {
            foreach (KeyValuePair<int, IBinder> pair in _registeredBinderObjects)
            {
                if (ReferenceEquals(binder, pair.Value))
                {
                    return pair.Key;
                }
            }

            return -1;
        }
    }

    private static IBinder GetBinderObjectById(int binderId)
    {
        lock (_lock)
        {
            if (_registeredBinderObjects.TryGetValue(binderId, out IBinder binder))
            {
                return binder;
            }

            return null;
        }
    }

    protected override ResultCode AdjustRefcount(int binderId, int addVal, int type)
    {
        IBinder binder = GetBinderObjectById(binderId);

        if (binder == null)
        {
            LogInvalidBinderId(binderId);

            return ResultCode.Success;
        }

        return binder.AdjustRefcount(addVal, type);
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.SurfaceFlinger, EventName = nameof(LogClass.SurfaceFlinger),
        Message = "Invalid binder id {binderId}.")]
    private partial void LogInvalidBinderId(int binderId);

    protected override void GetNativeHandle(int binderId, uint typeId, out KReadableEvent readableEvent)
    {
        IBinder binder = GetBinderObjectById(binderId);

        if (binder == null)
        {
            readableEvent = null;

            LogInvalidBinderId(binderId);

            return;
        }

        binder.GetNativeHandle(typeId, out readableEvent);
    }

    protected override ResultCode OnTransact(int binderId, uint code, uint flags, ReadOnlySpan<byte> inputParcel, Span<byte> outputParcel)
    {
        IBinder binder = GetBinderObjectById(binderId);

        if (binder == null)
        {
            LogInvalidBinderId(binderId);

            return ResultCode.Success;
        }

        return binder.OnTransact(code, flags, inputParcel, outputParcel);
    }
}