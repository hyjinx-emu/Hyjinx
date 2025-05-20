namespace Hyjinx.HLE.HOS.Services.Nfc
{
    [Service("nfc:am")]
    class IAmManager : IpcService<IAmManager>
    {
        public IAmManager(ServiceCtx context) { }
    }
}