namespace Hyjinx.HLE.HOS.Services.News
{
    [Service("news:a")]
    [Service("news:c")]
    [Service("news:m")]
    [Service("news:p")]
    [Service("news:v")]
    class IServiceCreator : IpcService<IServiceCreator>
    {
        public IServiceCreator(ServiceCtx context) { }
    }
}