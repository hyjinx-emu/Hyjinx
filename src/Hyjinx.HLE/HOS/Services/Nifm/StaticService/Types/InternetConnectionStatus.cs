using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Nifm.StaticService.Types;

[StructLayout(LayoutKind.Sequential)]
struct InternetConnectionStatus
{
    public InternetConnectionType Type;
    public byte WifiStrength;
    public InternetConnectionState State;
}