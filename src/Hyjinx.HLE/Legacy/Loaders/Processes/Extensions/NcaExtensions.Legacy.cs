#if IS_LEGACY_ENABLED

using LibHac.Tools.FsSystem.NcaUtils;

namespace Hyjinx.HLE.Loaders.Processes.Extensions;

partial class NcaExtensions
{
    public static bool IsMain(this Nca nca)
    {
        return nca.IsProgram() && !nca.IsPatch();
    }
}

#endif