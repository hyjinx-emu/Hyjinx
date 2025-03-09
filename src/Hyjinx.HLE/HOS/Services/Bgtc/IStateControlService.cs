namespace Hyjinx.HLE.HOS.Services.Bgct
{
    [Service("bgtc:sc")]
    class IStateControlService : IpcService<IStateControlService>
    {
        public IStateControlService(ServiceCtx context) { }
    }
}
