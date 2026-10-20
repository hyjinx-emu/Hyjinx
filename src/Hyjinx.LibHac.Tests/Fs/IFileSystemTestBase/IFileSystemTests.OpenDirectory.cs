using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void OpenDirectory_PathIsFile_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);

        using var directory = new UniqueRef<IDirectory>();
        Result res = fs.OpenDirectory(ref directory.Ref, "/file", OpenDirectoryMode.All);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void OpenDirectory_PathDoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        using var directory = new UniqueRef<IDirectory>();
        Result res = fs.OpenDirectory(ref directory.Ref, "/dir", OpenDirectoryMode.All);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }
}