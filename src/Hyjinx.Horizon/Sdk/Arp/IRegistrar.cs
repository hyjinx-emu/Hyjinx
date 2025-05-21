using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Ns;

namespace Hyjinx.Horizon.Sdk.Arp;

public interface IRegistrar
{
    public Result Issue(out ulong applicationInstanceId);
    public Result SetApplicationLaunchProperty(ApplicationLaunchProperty applicationLaunchProperty);
    public Result SetApplicationControlProperty(in ApplicationControlProperty applicationControlProperty);
}