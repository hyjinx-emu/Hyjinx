using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void CreateDirectory_EntryIsAdded()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "/dir"));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.Directory));
    }

    [Test]
    public void CreateDirectory_DirectoryExists_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");

        Result res = fs.CreateDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, res);
    }

    [Test]
    public void CreateDirectory_FileExists_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);

        Result res = fs.CreateDirectory("/file");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, res);
    }

    [Test]
    public void CreateDirectory_ParentDoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        Result res = fs.CreateFile("/dir1/dir2", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void CreateDirectory_WithTrailingSeparator_EntryIsAdded()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir/");
        Result res = fs.GetEntryType(out DirectoryEntryType type, "/dir/");

        LibHac.Tests.Fs.FsTestAssert.Success(res);
        Assert.That(type, Is.EqualTo(DirectoryEntryType.Directory));
    }

    [Test]
    public void CreateDirectory_MultipleSiblings()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        fs.CreateDirectory("/dir2");

        Result result1 = fs.GetEntryType(out DirectoryEntryType type1, "/dir1");
        Result result2 = fs.GetEntryType(out DirectoryEntryType type2, "/dir2");

        LibHac.Tests.Fs.FsTestAssert.Success(result1);
        LibHac.Tests.Fs.FsTestAssert.Success(result2);
        Assert.That(type1, Is.EqualTo(DirectoryEntryType.Directory));
        Assert.That(type2, Is.EqualTo(DirectoryEntryType.Directory));
    }

    [Test]
    public void CreateDirectory_InChildDirectory()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        fs.CreateDirectory("/dir2");

        fs.CreateDirectory("/dir1/dir1a");
        fs.CreateDirectory("/dir2/dir2a");

        Result result1 = fs.GetEntryType(out DirectoryEntryType type1, "/dir1/dir1a");
        Result result2 = fs.GetEntryType(out DirectoryEntryType type2, "/dir2/dir2a");

        LibHac.Tests.Fs.FsTestAssert.Success(result1);
        LibHac.Tests.Fs.FsTestAssert.Success(result2);
        Assert.That(type1, Is.EqualTo(DirectoryEntryType.Directory));
        Assert.That(type2, Is.EqualTo(DirectoryEntryType.Directory));
    }
}