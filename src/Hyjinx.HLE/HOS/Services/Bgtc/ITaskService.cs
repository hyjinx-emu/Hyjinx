namespace Hyjinx.HLE.HOS.Services.Bgct
{
    [Service("bgtc:t")]
    class ITaskService : IpcService<ITaskService>
    {
        public ITaskService(ServiceCtx context) { }
    }
}