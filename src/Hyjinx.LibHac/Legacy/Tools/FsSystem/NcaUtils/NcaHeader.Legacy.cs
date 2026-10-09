#if IS_LEGACY_ENABLED

using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaHeader
{
    public bool IsSectionEnabled(int index)
    {
        ref NcaSectionEntryStruct info = ref GetSectionEntry(index);

        int sectStart = info.StartBlock;
        int sectSize = info.EndBlock - sectStart;
        return sectStart != 0 || sectSize != 0;
    }
}

#endif