namespace Hyjinx.HLE.HOS.Services.Pm
{
    [Service("pm:bm")]
    class IBootModeInterface : IpcService<IBootModeInterface>
    {
        public IBootModeInterface(ServiceCtx context) { }
    }
}