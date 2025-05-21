using System;

namespace Hyjinx.HLE.HOS.Services.Sockets.Bsd.Types;

[Flags]
enum BsdSocketCreationFlags
{
    None = 0,
    CloseOnExecution = 1,
    NonBlocking = 2,

    FlagsShift = 28,
}