using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void OpenFile_PathIsDirectory_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");

        using var file = new UniqueRef<IFile>();
        Result res = fs.OpenFile(ref file.Ref, "/dir", OpenMode.All);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void OpenFile_PathDoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        using var file = new UniqueRef<IFile>();
        Result res = fs.OpenFile(ref file.Ref, "/file", OpenMode.All);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }
}