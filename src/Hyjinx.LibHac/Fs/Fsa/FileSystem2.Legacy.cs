#if IS_LEGACY_ENABLED

using LibHac.Common;
using System;

namespace LibHac.Fs.Fsa;

partial class FileSystem2 : FileSystem
{
    protected override Result DoCreateFile(in Path path, long size, CreateFileOptions option)
    {
        throw new NotSupportedException();
    }

    protected override Result DoDeleteFile(in Path path)
    {
        throw new NotSupportedException();
    }

    protected override Result DoCreateDirectory(in Path path)
    {
        throw new NotSupportedException();
    }

    protected override Result DoDeleteDirectory(in Path path)
    {
        throw new NotSupportedException();
    }

    protected override Result DoDeleteDirectoryRecursively(in Path path)
    {
        throw new NotSupportedException();
    }

    protected override Result DoCleanDirectoryRecursively(in Path path)
    {
        throw new NotSupportedException();
    }

    protected override Result DoRenameFile(in Path currentPath, in Path newPath)
    {
        throw new NotSupportedException();
    }

    protected override Result DoRenameDirectory(in Path currentPath, in Path newPath)
    {
        throw new NotSupportedException();
    }

    protected override Result DoGetEntryType(out DirectoryEntryType entryType, in Path path)
    {
        throw new NotSupportedException();
    }

    protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, in Path path, OpenMode mode)
    {
        throw new NotSupportedException();
    }

    protected override Result DoOpenDirectory(ref UniqueRef<IDirectory> outDirectory, in Path path, OpenDirectoryMode mode)
    {
        throw new NotSupportedException();
    }

    protected override Result DoCommit()
    {
        throw new NotSupportedException();
    }
}

#endif