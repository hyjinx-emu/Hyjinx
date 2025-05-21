using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Account;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Friends.Detail.Ipc;

interface IServiceCreator : IServiceObject
{
    Result CreateFriendService(out IFriendService friendService);
    Result CreateNotificationService(out INotificationService notificationService, Uid userId);
    Result CreateDaemonSuspendSessionService(out IDaemonSuspendSessionService daemonSuspendSessionService);
}