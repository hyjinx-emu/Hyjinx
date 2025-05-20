namespace Hyjinx.HLE.HOS.Services.Ldn
{
    [Service("ldn:s")]
    class ISystemServiceCreator : IpcService<ISystemServiceCreator>
    {
        public ISystemServiceCreator(ServiceCtx context) { }
    }
}