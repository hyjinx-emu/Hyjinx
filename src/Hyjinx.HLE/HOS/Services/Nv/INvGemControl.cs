namespace Hyjinx.HLE.HOS.Services.Nv;

[Service("nvgem:c")]
class INvGemControl : IpcService<INvGemControl>
{
    public INvGemControl(ServiceCtx context) { }
}