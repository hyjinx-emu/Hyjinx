#if IS_LEGACY_ENABLED

using LibHac.Fs;

// ReSharper disable once CheckNamespace
namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca1
{
    public IStorage OpenRawStorage(NcaSectionType type)
    {
        return OpenRawStorage(GetSectionIndexFromType(type));
    }

    private bool SectionExists(NcaSectionType type)
    {
        if (!TryGetSectionIndexFromType(type, Header.ContentType, out int index))
        {
            return false;
        }

        return SectionExists(index);
    }
}

#endif