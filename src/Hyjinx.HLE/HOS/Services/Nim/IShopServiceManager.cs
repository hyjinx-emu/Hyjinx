namespace Hyjinx.HLE.HOS.Services.Nim
{
    [Service("nim:shp")]
    class IShopServiceManager : IpcService<IShopServiceManager>
    {
        public IShopServiceManager(ServiceCtx context) { }
    }
}
