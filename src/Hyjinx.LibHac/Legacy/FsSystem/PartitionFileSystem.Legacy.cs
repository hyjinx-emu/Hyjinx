#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;

namespace LibHac.FsSystem;

partial class PartitionFileSystemCore<TMetaData, TFormat, THeader, TEntry>
{
    public Result GetFileBaseOffset(out long outOffset, U8Span path)
    {
        UnsafeHelpers.SkipParamInit(out outOffset);

        if (!_isInitialized)
            return ResultFs.PreconditionViolation.Log();

        if (path.Length == 0)
            return ResultFs.PathNotFound.Log();

        int entryIndex = _metaData.GetEntryIndex(path.Slice(1));
        if (entryIndex < 0)
            return ResultFs.PathNotFound.Log();

        outOffset = _metaDataSize + _metaData.GetEntry(entryIndex).Offset;
        return Result.Success;
    }
}

#endif