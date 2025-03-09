namespace Hyjinx.HLE.HOS.Services.Grc
{
    [Service("grc:d")] // 6.0.0+
    class IRemoteVideoTransfer : IpcService<IRemoteVideoTransfer>
    {
        public IRemoteVideoTransfer(ServiceCtx context) { }
    }
}
