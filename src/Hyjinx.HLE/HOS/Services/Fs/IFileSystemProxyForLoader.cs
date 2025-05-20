namespace Hyjinx.HLE.HOS.Services.Fs
{
    [Service("fsp-ldr")]
    class IFileSystemProxyForLoader : IpcService<IFileSystemProxyForLoader>
    {
        public IFileSystemProxyForLoader(ServiceCtx context) { }
    }
}