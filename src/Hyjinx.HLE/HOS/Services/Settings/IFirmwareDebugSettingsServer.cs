namespace Hyjinx.HLE.HOS.Services.Settings
{
    [Service("set:fd")]
    class IFirmwareDebugSettingsServer : IpcService<IFirmwareDebugSettingsServer>
    {
        public IFirmwareDebugSettingsServer(ServiceCtx context) { }
    }
}
