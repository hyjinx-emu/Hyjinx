namespace Hyjinx.HLE.HOS.Services.Eupld
{
    [Service("eupld:r")]
    class IRequest : IpcService<IRequest>
    {
        public IRequest(ServiceCtx context) { }
    }
}
