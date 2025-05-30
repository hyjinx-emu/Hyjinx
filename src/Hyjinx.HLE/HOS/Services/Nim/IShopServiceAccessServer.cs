using Hyjinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface.ShopServiceAccessServer;
using Hyjinx.Logging.Abstractions;

namespace Hyjinx.HLE.HOS.Services.Nim.ShopServiceAccessServerInterface;

class IShopServiceAccessServer : IpcService<IShopServiceAccessServer>
{
    public IShopServiceAccessServer() { }

    [CommandCmif(0)]
    // CreateAccessorInterface(u8) -> object<nn::ec::IShopServiceAccessor>
    public ResultCode CreateAccessorInterface(ServiceCtx context)
    {
        MakeObject(context, new IShopServiceAccessor(context.Device.System));

        // Logger.Stub?.PrintStub(LogClass.ServiceNim);

        return ResultCode.Success;
    }
}