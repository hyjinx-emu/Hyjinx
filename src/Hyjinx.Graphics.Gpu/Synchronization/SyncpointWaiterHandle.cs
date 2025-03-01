using System;

namespace Hyjinx.Graphics.Gpu.Synchronization
{
    public class SyncpointWaiterHandle
    {
        internal uint Threshold;
        internal Action<SyncpointWaiterHandle> Callback;
    }
}
