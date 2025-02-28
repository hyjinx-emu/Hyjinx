using Hyjinx.Horizon.Common;

namespace Hyjinx.Horizon.Sdk.Arp
{
    public interface IUnregistrationNotifier
    {
        public Result GetReadableHandle(out int readableHandle);
    }
}
