#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using System.IO;
using System.Linq;
using Path = LibHac.Fs.Path;

namespace LibHac.FsSystem;

partial class PartitionFileSystem2
{
    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        var fileName = path.ToString();

        var entry = _lookup.SingleOrDefault(o => o.FullName == fileName);
        if (entry == null)
        {
            throw new FileNotFoundException("The file does not exist.", fileName);
        }
        
        outFile.Reset(new StreamFile(new NxFileStream2(_baseStorage.Slice2(entry.Offset, entry.Size)), OpenMode.Read));
        return Result.Success;
    }
}

#endif