using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Friends.Detail.Ipc
{
    interface INotificationService : IServiceObject
    {
        Result GetEvent(out int eventHandle);
        Result Clear();
        Result Pop(out SizedNotificationInfo sizedNotificationInfo);
    }
}