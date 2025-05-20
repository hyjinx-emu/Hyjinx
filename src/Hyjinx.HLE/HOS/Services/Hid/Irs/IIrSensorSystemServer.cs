namespace Hyjinx.HLE.HOS.Services.Hid.Irs
{
    [Service("irs:sys")]
    class IIrSensorSystemServer : IpcService<IIrSensorSystemServer>
    {
        public IIrSensorSystemServer(ServiceCtx context) { }
    }
}