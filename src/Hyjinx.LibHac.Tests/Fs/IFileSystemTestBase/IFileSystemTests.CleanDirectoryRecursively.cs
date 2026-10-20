using LibHac.Fs;
using LibHac.Fs.Fsa;
using NUnit.Framework;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void CleanDirectoryRecursively_DeletesChildren()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");
        fs.CreateDirectory("/dir/dir2");
        fs.CreateFile("/dir/file1", 0, CreateFileOptions.None);

        Result resultDelete = fs.CleanDirectoryRecursively("/dir");

        Result resultDir1Type = fs.GetEntryType(out DirectoryEntryType dir1Type, "/dir");
        Result resultDir2Type = fs.GetEntryType(out _, "/dir/dir2");
        Result resultFileType = fs.GetEntryType(out _, "/dir/file1");

        LibHac.Tests.Fs.FsTestAssert.Success(resultDelete);

        LibHac.Tests.Fs.FsTestAssert.Success(resultDir1Type);
        Assert.That(dir1Type, Is.EqualTo(DirectoryEntryType.Directory));

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultDir2Type);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, resultFileType);
    }
}