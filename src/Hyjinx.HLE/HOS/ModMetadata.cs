using System.Collections.Generic;

namespace Hyjinx.HLE.HOS;

public struct ModMetadata
{
    public List<Mod> Mods { get; set; }

    public ModMetadata()
    {
        Mods = new List<Mod>();
    }
}