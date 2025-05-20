namespace Hyjinx.HLE.HOS.Services.Sockets.Bsd;

[Service("bsdcfg")]
class ServerInterface : IpcService<ServerInterface>
{
    public ServerInterface(ServiceCtx context) { }
}