using Hyjinx.Graphics.Gpu;
using Hyjinx.Graphics.Gpu.Synchronization;
using Hyjinx.HLE.HOS.Kernel;
using Hyjinx.HLE.HOS.Kernel.Threading;
using Hyjinx.HLE.HOS.Services.Nv.Types;
using Hyjinx.Horizon.Common;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl;

partial class NvHostEvent
{
    public NvFence Fence;
    public NvHostEventState State;
    public KEvent Event;
    public int EventHandle;

    private static readonly ILogger<NvHostEvent> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<NvHostEvent>();

    private readonly uint _eventId;
#pragma warning disable IDE0052 // Remove unread private member
    private readonly NvHostSyncpt _syncpointManager;
#pragma warning restore IDE0052
    private SyncpointWaiterHandle _waiterInformation;

    private NvFence _previousFailingFence;
    private uint _failingCount;

    public readonly object Lock = new();

    /// <summary>
    /// Max failing count until waiting on CPU.
    /// FIXME: This seems enough for most of the cases, reduce if needed.
    /// </summary>
    private const uint FailingCountMax = 2;

    public NvHostEvent(NvHostSyncpt syncpointManager, uint eventId, Horizon system)
    {
        Fence.Id = 0;

        State = NvHostEventState.Available;

        Event = new KEvent(system.KernelContext);

        if (KernelStatic.GetCurrentProcess().HandleTable.GenerateHandle(Event.ReadableEvent, out EventHandle) != Result.Success)
        {
            throw new InvalidOperationException("Out of handles!");
        }

        _eventId = eventId;

        _syncpointManager = syncpointManager;

        ResetFailingState();
    }

    private void ResetFailingState()
    {
        _previousFailingFence.Id = NvFence.InvalidSyncPointId;
        _previousFailingFence.Value = 0;
        _failingCount = 0;
    }

    private void Signal()
    {
        lock (Lock)
        {
            NvHostEventState oldState = State;

            State = NvHostEventState.Signaling;

            if (oldState == NvHostEventState.Waiting)
            {
                Event.WritableEvent.Signal();
            }

            State = NvHostEventState.Signaled;
        }
    }

    private void GpuSignaled(SyncpointWaiterHandle waiterInformation)
    {
        lock (Lock)
        {
            // If the signal does not match our current waiter,
            // then it is from a past fence and we should just ignore it.
            if (waiterInformation != null && waiterInformation != _waiterInformation)
            {
                return;
            }

            ResetFailingState();

            Signal();
        }
    }

    public void Cancel(GpuContext gpuContext)
    {
        lock (Lock)
        {
            NvHostEventState oldState = State;

            State = NvHostEventState.Cancelling;

            if (oldState == NvHostEventState.Waiting && _waiterInformation != null)
            {
                gpuContext.Synchronization.UnregisterCallback(Fence.Id, _waiterInformation);
                _waiterInformation = null;

                if (_previousFailingFence.Id == Fence.Id && _previousFailingFence.Value == Fence.Value)
                {
                    _failingCount++;
                }
                else
                {
                    _failingCount = 1;

                    _previousFailingFence = Fence;
                }
            }

            State = NvHostEventState.Cancelled;

            Event.WritableEvent.Clear();
        }
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
        Message = "GPU processing thread is too slow, waiting on CPU...")]
    private partial void LogGpuProcessingThreadTooSlow();

    public bool Wait(GpuContext gpuContext, NvFence fence)
    {
        lock (Lock)
        {
            // NOTE: nvservices code should always wait on the GPU side.
            //       If we do this, we may get an abort or undefined behaviour when the GPU processing thread is blocked for a long period (for example, during shader compilation).
            //       The reason for this is that the NVN code will try to wait until giving up.
            //       This is done by trying to wait and signal multiple times until aborting after you are past the timeout.
            //       As such, if it fails too many time, we enforce a wait on the CPU side indefinitely.
            //       This allows to keep GPU and CPU in sync when we are slow.
            if (_failingCount == FailingCountMax)
            {
                LogGpuProcessingThreadTooSlow();

                Fence.Wait(gpuContext, Timeout.InfiniteTimeSpan);

                ResetFailingState();

                return false;
            }
            else
            {
                Fence = fence;
                State = NvHostEventState.Waiting;

                _waiterInformation = gpuContext.Synchronization.RegisterCallbackOnSyncpoint(Fence.Id, Fence.Value, GpuSignaled);

                return true;
            }
        }
    }

    public string DumpState(GpuContext gpuContext)
    {
        string res = $"\nNvHostEvent {_eventId}:\n";
        res += $"\tState: {State}\n";

        if (State == NvHostEventState.Waiting)
        {
            res += "\tFence:\n";
            res += $"\t\tId            : {Fence.Id}\n";
            res += $"\t\tThreshold     : {Fence.Value}\n";
            res += $"\t\tCurrent Value : {gpuContext.Synchronization.GetSyncpointValue(Fence.Id)}\n";
            res += $"\t\tWaiter Valid  : {_waiterInformation != null}\n";
        }

        return res;
    }

    public void CloseEvent(ServiceCtx context)
    {
        if (EventHandle != 0)
        {
            context.Process.HandleTable.CloseHandle(EventHandle);
            EventHandle = 0;
        }
    }
}