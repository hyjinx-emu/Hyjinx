namespace Hyjinx.HLE.HOS.Services.Nfc.Mifare;

[Service("nfc:mf:u")]
class IUserManager : IpcService<IUserManager>
{
    public IUserManager(ServiceCtx context) { }
}