using LibHac.Ncm;
using Hyjinx.HLE.HOS.Services.Ncm.Lr.LocationResolverManager;

namespace Hyjinx.HLE.HOS.Services.Ncm.Lr
{
    [Service("lr")]
    class ILocationResolverManager : IpcService<ILocationResolverManager>
    {
        public ILocationResolverManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // OpenLocationResolver()
        public ResultCode OpenLocationResolver(ServiceCtx context)
        {
            StorageId storageId = (StorageId)context.RequestData.ReadByte();

            MakeObject(context, new ILocationResolver(storageId));

            return ResultCode.Success;
        }
    }
}
