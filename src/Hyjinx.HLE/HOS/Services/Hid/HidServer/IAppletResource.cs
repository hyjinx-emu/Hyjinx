using Hyjinx.HLE.HOS.Ipc;
using Hyjinx.HLE.HOS.Kernel.Memory;
using Hyjinx.Horizon.Common;
using System;

namespace Hyjinx.HLE.HOS.Services.Hid.HidServer;

class IAppletResource : IpcService<IAppletResource>
{
    private readonly KSharedMemory _hidSharedMem;
    private int _hidSharedMemHandle;

    public IAppletResource(KSharedMemory hidSharedMem)
    {
        _hidSharedMem = hidSharedMem;
    }

    [CommandCmif(0)]
    // GetSharedMemoryHandle() -> handle<copy>
    public ResultCode GetSharedMemoryHandle(ServiceCtx context)
    {
        if (_hidSharedMemHandle == 0)
        {
            if (context.Process.HandleTable.GenerateHandle(_hidSharedMem, out _hidSharedMemHandle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }
        }

        context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_hidSharedMemHandle);

        return ResultCode.Success;
    }
}