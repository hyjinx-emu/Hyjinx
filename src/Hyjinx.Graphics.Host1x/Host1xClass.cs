using Hyjinx.Graphics.Device;
using System.Collections.Generic;
using System.Threading;

namespace Hyjinx.Graphics.Host1x;

public class Host1xClass : IDeviceState
{
    private readonly ISynchronizationManager _syncMgr;
    private readonly DeviceState<Host1xClassRegisters> _state;

    public Host1xClass(ISynchronizationManager syncMgr)
    {
        _syncMgr = syncMgr;
        _state = new DeviceState<Host1xClassRegisters>(new Dictionary<string, RwCallback>
        {
            { nameof(Host1xClassRegisters.WaitSyncpt32), new RwCallback(WaitSyncpt32, null) },
        });
    }

    public int Read(int offset) => _state.Read(offset);
    public void Write(int offset, int data) => _state.Write(offset, data);

    private void WaitSyncpt32(int data)
    {
        uint syncpointId = (uint)(data & 0xFF);
        uint threshold = _state.State.LoadSyncptPayload32;

        _syncMgr.WaitOnSyncpoint(syncpointId, threshold, Timeout.InfiniteTimeSpan);
    }
}