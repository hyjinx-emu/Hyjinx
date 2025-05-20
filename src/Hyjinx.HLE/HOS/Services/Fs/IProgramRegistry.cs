namespace Hyjinx.HLE.HOS.Services.Fs
{
    [Service("fsp-pr")]
    class IProgramRegistry : IpcService<IProgramRegistry>
    {
        public IProgramRegistry(ServiceCtx context) { }
    }
}