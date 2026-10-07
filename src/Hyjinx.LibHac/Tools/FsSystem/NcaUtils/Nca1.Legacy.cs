#if IS_LEGACY_ENABLED

// ReSharper disable once CheckNamespace
namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca1
{
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