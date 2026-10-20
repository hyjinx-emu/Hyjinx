using LibHac.Fs;
using LibHac.Fs.Fsa;
using NUnit.Framework;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void GetEntryType_RootIsDirectory()
    {
        IFileSystem fs = CreateFileSystem();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "/"u8));

        Assert.That(type, Is.EqualTo(DirectoryEntryType.Directory));
    }

    [Test]
    public void GetEntryType_PathDoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, fs.GetEntryType(out _, "/path"u8));
    }
}