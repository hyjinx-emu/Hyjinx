#if IS_LEGACY_ENABLED

using LibHac.Fs;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca
{
    public IStorage OpenRawStorage(NcaSectionType type)
    {
        return OpenRawStorage(GetSectionIndexFromType(type));
    }
}

#endif
