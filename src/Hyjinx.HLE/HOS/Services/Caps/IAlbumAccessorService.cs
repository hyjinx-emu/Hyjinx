namespace Hyjinx.HLE.HOS.Services.Caps
{
    [Service("caps:a")]
    class IAlbumAccessorService : IpcService<IAlbumAccessorService>
    {
        public IAlbumAccessorService(ServiceCtx context) { }
    }
}
