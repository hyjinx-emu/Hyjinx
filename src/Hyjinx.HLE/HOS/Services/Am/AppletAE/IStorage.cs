namespace Hyjinx.HLE.HOS.Services.Am.AppletAE;

class IStorage : IpcService<IStorage>
{
    public bool IsReadOnly { get; private set; }
    public byte[] Data { get; private set; }

    public IStorage(byte[] data, bool isReadOnly = false)
    {
        IsReadOnly = isReadOnly;
        Data = data;
    }

    [CommandCmif(0)]
    // Open() -> object<nn::am::service::IStorageAccessor>
    public ResultCode Open(ServiceCtx context)
    {
        MakeObject(context, new IStorageAccessor(this));

        return ResultCode.Success;
    }
}