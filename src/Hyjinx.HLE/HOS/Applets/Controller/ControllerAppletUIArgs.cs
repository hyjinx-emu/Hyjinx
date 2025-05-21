using Hyjinx.HLE.HOS.Services.Hid;
using System.Collections.Generic;

namespace Hyjinx.HLE.HOS.Applets;

public struct ControllerAppletUIArgs
{
    public int PlayerCountMin;
    public int PlayerCountMax;
    public ControllerType SupportedStyles;
    public IEnumerable<PlayerIndex> SupportedPlayers;
    public bool IsDocked;
}