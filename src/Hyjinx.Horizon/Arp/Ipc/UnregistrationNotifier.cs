using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Arp;
using Hyjinx.Horizon.Sdk.Arp.Detail;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Arp.Ipc
{
    partial class UnregistrationNotifier : IUnregistrationNotifier, IServiceObject
    {
        private readonly ApplicationInstanceManager _applicationInstanceManager;

        public UnregistrationNotifier(ApplicationInstanceManager applicationInstanceManager)
        {
            _applicationInstanceManager = applicationInstanceManager;
        }

        [CmifCommand(0)]
        public Result GetReadableHandle([CopyHandle] out int readableHandle)
        {
            readableHandle = _applicationInstanceManager.EventHandle;

            return Result.Success;
        }
    }
}