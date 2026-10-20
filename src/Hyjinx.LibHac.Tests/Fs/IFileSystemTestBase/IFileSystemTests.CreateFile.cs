using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void CreateFile_EntryIsAdded()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);
        Result res = fs.GetEntryType(out DirectoryEntryType type, "/file");

        LibHac.Tests.Fs.FsTestAssert.Success(res);
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void CreateFile_DirectoryExists_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir");

        Result res = fs.CreateFile("/dir", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, res);
    }

    [Test]
    public void CreateFile_FileExists_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);

        Result res = fs.CreateFile("/file", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, res);
    }

    [Test]
    public void CreateFile_ParentDoesNotExist_ReturnsPathNotFound()
    {
        IFileSystem fs = CreateFileSystem();

        Result res = fs.CreateFile("/dir/file", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void CreateFile_WithTrailingSeparator_EntryIsAdded()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file/", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "/file/"));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void CreateFile_WithSize_SizeIsSet()
    {
        const long expectedSize = 12345;

        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", expectedSize, CreateFileOptions.None);

        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Read);

        LibHac.Tests.Fs.FsTestAssert.Success(file.Get.GetSize(out long fileSize));
        Assert.That(fileSize, Is.EqualTo(expectedSize));
    }

    [Test]
    public void CreateFile_MultipleSiblings()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file1", 0, CreateFileOptions.None);
        fs.CreateFile("/file2", 0, CreateFileOptions.None);

        Result result1 = fs.GetEntryType(out DirectoryEntryType type1, "/file1");
        Result result2 = fs.GetEntryType(out DirectoryEntryType type2, "/file2");

        LibHac.Tests.Fs.FsTestAssert.Success(result1);
        LibHac.Tests.Fs.FsTestAssert.Success(result2);
        Assert.That(type1, Is.EqualTo(DirectoryEntryType.File));
        Assert.That(type2, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void CreateFile_InChildDirectory()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateDirectory("/dir1");
        fs.CreateDirectory("/dir2");

        fs.CreateFile("/dir1/file1", 0, CreateFileOptions.None);
        fs.CreateFile("/dir2/file2", 0, CreateFileOptions.None);

        Result result1 = fs.GetEntryType(out DirectoryEntryType type1, "/dir1/file1");
        Result result2 = fs.GetEntryType(out DirectoryEntryType type2, "/dir2/file2");

        LibHac.Tests.Fs.FsTestAssert.Success(result1);
        LibHac.Tests.Fs.FsTestAssert.Success(result2);
        Assert.That(type1, Is.EqualTo(DirectoryEntryType.File));
        Assert.That(type2, Is.EqualTo(DirectoryEntryType.File));
    }
}