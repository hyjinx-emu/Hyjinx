namespace Hyjinx.HLE.HOS.Services.Ptm.Pcm;

[Service("pcm")]
class IManager : IpcService<IManager>
{
    public IManager(ServiceCtx context) { }
}