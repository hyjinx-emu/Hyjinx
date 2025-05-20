namespace Hyjinx.HLE.HOS.Services.Nv
{
    [Service("nvgem:cd")]
    class INvGemCoreDump : IpcService<INvGemCoreDump>
    {
        public INvGemCoreDump(ServiceCtx context) { }
    }
}