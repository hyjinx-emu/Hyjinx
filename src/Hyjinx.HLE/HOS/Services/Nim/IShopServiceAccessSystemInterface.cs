namespace Hyjinx.HLE.HOS.Services.Nim
{
    [Service("nim:ecas")] // 7.0.0+
    class IShopServiceAccessSystemInterface : IpcService<IShopServiceAccessSystemInterface>
    {
        public IShopServiceAccessSystemInterface(ServiceCtx context) { }
    }
}