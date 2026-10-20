using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract class IAttributeFileSystemTests : IFileSystemTests
{
    protected abstract IAttributeFileSystem CreateAttributeFileSystem();

    [Test]
    public void CreateDirectory_WithoutArchiveAttribute_ArchiveFlagIsNotSet()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateDirectory("/dir", NxFileAttributes.None));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetFileAttributes(out NxFileAttributes attributes, "/dir"));
        Assert.That(attributes, Is.EqualTo(NxFileAttributes.Directory));
    }

    [Test]
    public void CreateDirectory_WithArchiveAttribute_ArchiveFlagIsSet()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateDirectory("/dir", NxFileAttributes.Archive));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetFileAttributes(out NxFileAttributes attributes, "/dir"));
        Assert.That(attributes, Is.EqualTo(NxFileAttributes.Directory | NxFileAttributes.Archive));
    }

    [Test]
    public void GetFileAttributes_AttributesOnNewFileAreEmpty()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();
        fs.CreateFile("/file", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetFileAttributes(out NxFileAttributes attributes, "/file"));
        Assert.That(attributes, Is.EqualTo(NxFileAttributes.None));
    }

    [Test]
    public void GetFileAttributes_AttributesOnNewDirHaveOnlyDirFlagSet()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();
        fs.CreateDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetFileAttributes(out NxFileAttributes attributes, "/dir"));
        Assert.That(attributes, Is.EqualTo(NxFileAttributes.Directory));
    }

    [Test]
    public void GetFileAttributes_PathDoesNotExist_ReturnsPathNotFound()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();

        Result res = fs.GetFileAttributes(out _, "/path");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void SetFileAttributes_PathDoesNotExist_ReturnsPathNotFound()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();

        Result res = fs.SetFileAttributes("/path", NxFileAttributes.None);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void SetFileAttributes_SetAttributeOnFile()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();
        fs.CreateFile("/file", 0, CreateFileOptions.None);

        Result resultSet = fs.SetFileAttributes("/file", NxFileAttributes.Archive);
        Result resultGet = fs.GetFileAttributes(out NxFileAttributes attributes, "/file");

        LibHac.Tests.Fs.FsTestAssert.Success(resultSet);
        LibHac.Tests.Fs.FsTestAssert.Success(resultGet);
        Assert.That(attributes, Is.EqualTo(NxFileAttributes.Archive));
    }

    [Test]
    public void SetFileAttributes_SetAttributeOnDirectory()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();
        fs.CreateDirectory("/dir");

        Result resultSet = fs.SetFileAttributes("/dir", NxFileAttributes.Archive);
        Result resultGet = fs.GetFileAttributes(out NxFileAttributes attributes, "/dir");

        LibHac.Tests.Fs.FsTestAssert.Success(resultSet);
        LibHac.Tests.Fs.FsTestAssert.Success(resultGet);
        Assert.That(attributes, Is.EqualTo(NxFileAttributes.Directory | NxFileAttributes.Archive));
    }

    [Test]
    public void GetFileSize_ReadNewFileSize()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();

        fs.CreateFile("/file", 845, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetFileSize(out long fileSize, "/file"));
        Assert.That(fileSize, Is.EqualTo(845));
    }

    [Test]
    public void GetFileSize_PathDoesNotExist_ReturnsPathNotFound()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();

        Result res = fs.GetFileSize(out _, "/path");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void GetFileSize_PathIsDirectory_ReturnsPathNotFound()
    {
        IAttributeFileSystem fs = CreateAttributeFileSystem();
        fs.CreateDirectory("/dir");

        Result res = fs.GetFileSize(out _, "/dir");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }
}