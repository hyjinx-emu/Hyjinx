namespace Hyjinx.HLE.HOS.Services.Am.Idle
{
    [Service("idle:sys")]
    class IPolicyManagerSystem : IpcService<IPolicyManagerSystem>
    {
        public IPolicyManagerSystem(ServiceCtx context) { }
    }
}
