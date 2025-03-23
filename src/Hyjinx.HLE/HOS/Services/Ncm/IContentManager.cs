namespace Hyjinx.HLE.HOS.Services.Ncm
{
    [Service("ncm")]
    class IContentManager : IpcService<IContentManager>
    {
        public IContentManager(ServiceCtx context) { }
    }
}
