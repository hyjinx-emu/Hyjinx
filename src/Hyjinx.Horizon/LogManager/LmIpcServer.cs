using Hyjinx.Horizon.LogManager.Ipc;
using Hyjinx.Horizon.Sdk.Sf.Hipc;
using Hyjinx.Horizon.Sdk.Sm;

namespace Hyjinx.Horizon.LogManager
{
    class LmIpcServer
    {
        private const int MaxSessionsCount = 42;

        private const int PointerBufferSize = 0x400;
        private const int MaxDomains = 31;
        private const int MaxDomainObjects = 61;
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

            _serverManager.RegisterObjectForServer(new LogService(), ServiceName.Encode("lm"), MaxSessionsCount);
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
