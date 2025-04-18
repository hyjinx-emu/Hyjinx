#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaExtensions
{
    public static IStorage OpenStorage(this Nca nca, int index, IntegrityCheckLevel integrityCheckLevel, bool openRaw)
    {
        if (openRaw) return nca.OpenRawStorage(index);
        return nca.OpenStorage(index, integrityCheckLevel);
    }
    
    public static IStorage OpenStorage(this Nca nca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel, bool openRaw)
    {
        if (openRaw) return nca.OpenRawStorage(type);
        return nca.OpenStorage(type, integrityCheckLevel);
    }
}

#endif
