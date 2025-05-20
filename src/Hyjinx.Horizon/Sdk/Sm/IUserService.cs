using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Sm;

interface IUserService : IServiceObject
{
    Result Initialize(ulong clientProcessId);
    Result GetService(out int handle, ServiceName name);
    Result RegisterService(out int handle, ServiceName name, int maxSessions, bool isLight);
    Result UnregisterService(ServiceName name);
}