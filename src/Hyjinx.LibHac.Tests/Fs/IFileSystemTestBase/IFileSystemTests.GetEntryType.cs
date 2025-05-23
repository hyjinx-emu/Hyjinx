using LibHac.Fs;
using LibHac.Fs.Fsa;
using Xunit;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Fact]
    public void GetEntryType_RootIsDirectory()
    {
        IFileSystem fs = CreateFileSystem();

        Assert.Success(fs.GetEntryType(out DirectoryEntryType type, "/"u8));

        Assert.Equal(DirectoryEntryType.Directory, type);
    }

    [Fact]
    public void GetEntryType_PathDoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        Assert.Result(ResultFs.PathNotFound, fs.GetEntryType(out _, "/path"u8));
    }
}