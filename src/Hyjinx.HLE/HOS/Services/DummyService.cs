namespace Hyjinx.HLE.HOS.Services;

class DummyService : IpcService<DummyService>
{
    public string ServiceName { get; set; }

    public DummyService(string serviceName)
    {
        ServiceName = serviceName;
    }
}