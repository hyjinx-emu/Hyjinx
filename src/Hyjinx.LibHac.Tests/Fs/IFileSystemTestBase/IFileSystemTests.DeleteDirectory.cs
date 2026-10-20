using LibHac.Fs;
using LibHac.Fs.Fsa;
using NUnit.Framework;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void DeleteDirectory_DoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        Result res = fs.DeleteDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void DeleteDirectory_DirectoryExists_EntryIsRemoved()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");

        Result resultDelete = fs.DeleteDirectory("/dir");
        Result resultEntry = fs.GetEntryType(out _, "/dir");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultEntry);
    }

    [Test]
    public void DeleteDirectory_PathIsFile_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);

        Result res = fs.DeleteDirectory("/file");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void DeleteDirectory_HasOlderSibling_SiblingNotDeleted()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        fs.CreateDirectory("/dir2");

        Result resultDelete = fs.DeleteDirectory("/dir2");
        Result resultEntry1 = fs.GetEntryType(out DirectoryEntryType dir1Type, "/dir1");
        Result resultEntry2 = fs.GetEntryType(out _, "/dir2");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);
        LibHac.Tests.Fs.FsTestAssert.Success(resultEntry1);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultEntry2);

        Assert.That(dir1Type, Is.EqualTo(DirectoryEntryType.Directory));
    }

    [Test]
    public void DeleteDirectory_HasYoungerSibling_SiblingNotDeleted()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir2");
        fs.CreateDirectory("/dir1");

        Result resultDelete = fs.DeleteDirectory("/dir2");
        Result resultEntry1 = fs.GetEntryType(out DirectoryEntryType dir1Type, "/dir1");
        Result resultEntry2 = fs.GetEntryType(out _, "/dir2");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);
        LibHac.Tests.Fs.FsTestAssert.Success(resultEntry1);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultEntry2);

        Assert.That(dir1Type, Is.EqualTo(DirectoryEntryType.Directory));
    }

    [Test]
    public void DeleteDirectory_NotEmpty_ReturnsDirectoryNotEmpty()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");
        fs.CreateFile("/dir/file", 0, CreateFileOptions.None);

        Result res = fs.DeleteDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.DirectoryNotEmpty, res);
    }
}