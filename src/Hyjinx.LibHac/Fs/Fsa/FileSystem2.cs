using LibHac.Common;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Represents a file system.
/// </summary>
public abstract class FileSystem2 : FileSystem
{
    protected override Result DoCreateFile(in Path path, long size, CreateFileOptions option)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoDeleteFile(in Path path)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoCreateDirectory(in Path path)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoDeleteDirectory(in Path path)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoDeleteDirectoryRecursively(in Path path)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoCleanDirectoryRecursively(in Path path)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoRenameFile(in Path currentPath, in Path newPath)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoRenameDirectory(in Path currentPath, in Path newPath)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoGetEntryType(out DirectoryEntryType entryType, in Path path)
    {
        entryType = default;
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoOpenDirectory(ref UniqueRef<IDirectory> outDirectory, in Path path, OpenDirectoryMode mode)
    {
        return ResultFs.NotImplemented.Log();
    }

    protected override Result DoCommit()
    {
        return ResultFs.NotImplemented.Log();
    }
}