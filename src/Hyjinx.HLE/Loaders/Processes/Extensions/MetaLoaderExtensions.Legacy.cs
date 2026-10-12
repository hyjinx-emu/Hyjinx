#if IS_LEGACY_ENABLED

using LibHac.Loader;
using LibHac.Util;

namespace Hyjinx.HLE.Loaders.Processes.Extensions;

partial class MetaLoaderExtensions
{
    public static string GetProgramName(this MetaLoader metaLoader)
    {
        metaLoader.GetNpdm(out var npdm).ThrowIfFailure();

        return StringUtils.Utf8ZToString(npdm.Meta.ProgramName);
    }
}

#endif