using System.Runtime.InteropServices;

namespace Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator.Types;

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
struct NetworkErrorMessage
{
    public NetworkError Error;
}