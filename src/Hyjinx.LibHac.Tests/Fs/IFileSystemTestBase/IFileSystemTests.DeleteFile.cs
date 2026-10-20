using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void DeleteFile_DoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        Result res = fs.DeleteFile("/file");
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void DeleteFile_FileExists_FileEntryIsRemoved()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);

        Result resultDelete = fs.DeleteFile("/file");
        Result resultEntry = fs.GetEntryType(out _, "/file");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultEntry);
    }

    [Test]
    public void DeleteFile_PathIsDirectory_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");

        Result res = fs.DeleteFile("/dir");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void DeleteFile_HasOlderSibling_SiblingNotDeleted()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file1", 0, CreateFileOptions.None);
        fs.CreateFile("/file2", 0, CreateFileOptions.None);

        Result resultDelete = fs.DeleteFile("/file2");
        Result resultEntry1 = fs.GetEntryType(out DirectoryEntryType dir1Type, "/file1");
        Result resultEntry2 = fs.GetEntryType(out _, "/file2");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);
        LibHac.Tests.Fs.FsTestAssert.Success(resultEntry1);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultEntry2);

        Assert.That(dir1Type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void DeleteFile_HasYoungerSibling_SiblingNotDeleted()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file2", 0, CreateFileOptions.None);
        fs.CreateFile("/file1", 0, CreateFileOptions.None);

        Result resultDelete = fs.DeleteFile("/file2");
        Result resultEntry1 = fs.GetEntryType(out DirectoryEntryType dir1Type, "/file1");
        Result resultEntry2 = fs.GetEntryType(out _, "/file2");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);
        LibHac.Tests.Fs.FsTestAssert.Success(resultEntry1);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultEntry2);

        Assert.That(dir1Type, Is.EqualTo(DirectoryEntryType.File));
    }
}