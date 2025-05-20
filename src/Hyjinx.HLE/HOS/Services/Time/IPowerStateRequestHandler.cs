namespace Hyjinx.HLE.HOS.Services.Time
{
    [Service("time:p")] // 9.0.0+
    class IPowerStateRequestHandler : IpcService<IPowerStateRequestHandler>
    {
        public IPowerStateRequestHandler(ServiceCtx context) { }
    }
}