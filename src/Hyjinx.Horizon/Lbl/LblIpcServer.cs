using Hyjinx.Horizon.Lbl.Ipc;
using Hyjinx.Horizon.Sdk.Sf.Hipc;
using Hyjinx.Horizon.Sdk.Sm;

namespace Hyjinx.Horizon.Lbl
{
    class LblIpcServer
    {
        private const int MaxSessionsCount = 5;

        private const int PointerBufferSize = 0;
        private const int MaxDomains = 0;
        private const int MaxDomainObjects = 0;
        private const int MaxPortsCount = 1;

        private static readonly ManagerOptions _managerOptions = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _managerOptions, MaxSessionsCount);

            _serverManager.RegisterObjectForServer(new LblController(), ServiceName.Encode("lbl"), MaxSessionsCount);
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _serverManager.Dispose();
            _sm.Dispose();
        }
    }
}
