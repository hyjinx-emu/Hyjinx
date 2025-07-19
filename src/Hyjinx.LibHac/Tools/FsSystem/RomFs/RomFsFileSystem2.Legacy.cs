#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using System.Linq;

namespace LibHac.Tools.FsSystem.RomFs;

partial class RomFsFileSystem2
{
    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        var parts = path.ToString().Split('/');

        var entry = _filesIndex.Enumerate(0).Single(o => o.Name == parts[^1]);

        outFile.Reset(new RomFsFile(_baseStorage, _dataOffset + entry.Info.Offset, entry.Info.Length));
        return Result.Success;
    }
}

#endif