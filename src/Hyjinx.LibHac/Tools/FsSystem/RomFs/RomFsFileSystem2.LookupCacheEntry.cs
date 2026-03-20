using LibHac.Fs;

namespace LibHac.Tools.FsSystem.RomFs;

partial class RomFsFileSystem2
{
    /// <summary>
    /// Describes an entry within the lookup cache.
    /// </summary>
    private class LookupCacheEntry
    {
        public long Offset { get; init; }
        public long Length { get; init; }
        public int FirstFileOffset { get; init; }
        public int FirstSubDirectoryOffset { get; init; }
    }
}