using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.Collections.Generic;
using System.IO;

namespace LibHac.FsSystem;

public class Sha256PartitionFileSystem2 : FileSystem2
{
    public override Stream OpenFile(string fileName, FileAccess access = FileAccess.Read)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<FileInfoEx> EnumerateFileInfos(string? path = null, string? searchPattern = null,
        SearchOptions options = SearchOptions.Default)
    {
        throw new System.NotImplementedException();
    }
}