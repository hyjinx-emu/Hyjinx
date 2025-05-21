using Hyjinx.HLE.HOS.Ipc;
using Hyjinx.HLE.HOS.Kernel.Threading;
using Hyjinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface.ShopServiceAccessServer.ShopServiceAccessor;
using Hyjinx.Horizon.Common;
using Hyjinx.Logging.Abstractions;
using System;

namespace Hyjinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface.ShopServiceAccessServer;

class IShopServiceAccessor : IpcService<IShopServiceAccessor>
{
    private readonly KEvent _event;

    private int _eventHandle;

    public IShopServiceAccessor(Horizon system)
    {
        _event = new KEvent(system.KernelContext);
    }

    [CommandCmif(0)]
    // CreateAsyncInterface(u64) -> (handle<copy>, object<nn::ec::IShopServiceAsync>)
    public ResultCode CreateAsyncInterface(ServiceCtx context)
    {
        MakeObject(context, new IShopServiceAsync());

        if (_eventHandle == 0)
        {
            if (context.Process.HandleTable.GenerateHandle(_event.ReadableEvent, out _eventHandle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }
        }

        context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_eventHandle);

        // Logger.Stub?.PrintStub(LogClass.ServiceNim);

        return ResultCode.Success;
    }
}