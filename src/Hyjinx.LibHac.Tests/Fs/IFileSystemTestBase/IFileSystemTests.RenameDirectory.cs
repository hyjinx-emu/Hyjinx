using LibHac.Fs;
using LibHac.Fs.Fsa;
using NUnit.Framework;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void RenameDirectory_EntriesAreMoved()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        Result resultRename = fs.RenameDirectory("/dir1", "/dir2");

        Result resultDir2 = fs.GetEntryType(out DirectoryEntryType dir2Type, "/dir2");
        Result resultDir1 = fs.GetEntryType(out _, "/dir1");

        LibHac.Tests.Fs.FsTestAssert.Success(resultRename);

        LibHac.Tests.Fs.FsTestAssert.Success(resultDir2);
        Assert.That(dir2Type, Is.EqualTo(DirectoryEntryType.Directory));

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultDir1);
    }

    [Test]
    public void RenameDirectory_HasChildren_NewChildPathExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        fs.CreateDirectory("/dir1/dirC");
        fs.CreateFile("/dir1/file1", 0, CreateFileOptions.None);

        Result resultRename = fs.RenameDirectory("/dir1", "/dir2");

        // Check that renamed structure exists
        Result resultDir2 = fs.GetEntryType(out DirectoryEntryType dir2Type, "/dir2");
        Result resultDirC = fs.GetEntryType(out DirectoryEntryType dir1CType, "/dir2/dirC");
        Result resultFile1 = fs.GetEntryType(out DirectoryEntryType file1Type, "/dir2/file1");

        // Check that old structure doesn't exist
        Result resultDir1 = fs.GetEntryType(out _, "/dir1");
        Result resultDirCOld = fs.GetEntryType(out _, "/dir1/dirC");
        Result resultFile1Old = fs.GetEntryType(out _, "/dir1/file1");

        LibHac.Tests.Fs.FsTestAssert.Success(resultRename);

        LibHac.Tests.Fs.FsTestAssert.Success(resultDir2);
        LibHac.Tests.Fs.FsTestAssert.Success(resultDirC);
        LibHac.Tests.Fs.FsTestAssert.Success(resultFile1);

        Assert.That(dir2Type, Is.EqualTo(DirectoryEntryType.Directory));
        Assert.That(dir1CType, Is.EqualTo(DirectoryEntryType.Directory));
        Assert.That(file1Type, Is.EqualTo(DirectoryEntryType.File));

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultDir1);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultDirCOld);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultFile1Old);
    }

    [Test]
    public void RenameDirectory_DestHasDifferentParentDirectory()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/parent1");
        fs.CreateDirectory("/parent2");
        fs.CreateDirectory("/parent1/dir1");

        Result resultRename = fs.RenameDirectory("/parent1/dir1", "/parent2/dir2");

        Result resultDir2 = fs.GetEntryType(out DirectoryEntryType dir2Type, "/parent2/dir2");
        Result resultDir1 = fs.GetEntryType(out _, "/parent1/dir1");

        LibHac.Tests.Fs.FsTestAssert.Success(resultRename);

        Assert.That(resultDir2, Is.EqualTo(Result.Success));
        LibHac.Tests.Fs.FsTestAssert.Success(resultDir2);
        Assert.That(dir2Type, Is.EqualTo(DirectoryEntryType.Directory));

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultDir1);
    }

    [Test]
    public void RenameDirectory_DestExists_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        fs.CreateDirectory("/dir2");

        Result resultRename = fs.RenameDirectory("/dir1", "/dir2");

        Result resultDir1 = fs.GetEntryType(out DirectoryEntryType dir1Type, "/dir1");
        Result resultDir2 = fs.GetEntryType(out DirectoryEntryType dir2Type, "/dir2");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, resultRename);

        LibHac.Tests.Fs.FsTestAssert.Success(resultDir1);
        LibHac.Tests.Fs.FsTestAssert.Success(resultDir2);
        Assert.That(dir1Type, Is.EqualTo(DirectoryEntryType.Directory));
        Assert.That(dir2Type, Is.EqualTo(DirectoryEntryType.Directory));
    }
}