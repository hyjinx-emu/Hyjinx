namespace Hyjinx.HLE.HOS.Services.Fs
{
    [Service("fsp-ldr")]
    class IFileSystemProxyForLoader : IpcService
    {
        public IFileSystemProxyForLoader(ServiceCtx context) { }
    }
}
