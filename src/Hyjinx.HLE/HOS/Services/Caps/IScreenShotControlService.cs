namespace Hyjinx.HLE.HOS.Services.Caps
{
    [Service("caps:sc")]
    class IScreenShotControlService : IpcService<IScreenShotControlService>
    {
        public IScreenShotControlService(ServiceCtx context) { }
    }
}
