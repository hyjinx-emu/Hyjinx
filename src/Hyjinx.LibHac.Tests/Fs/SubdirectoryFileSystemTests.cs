using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tests.Fs.IFileSystemTestBase;
using LibHac.Tools.Fs;
using NUnit.Framework;

namespace LibHac.Tests.Fs;

public class SubdirectoryFileSystemTests : IFileSystemTests
{
    protected override IFileSystem CreateFileSystem()
    {
        return CreateFileSystemInternal().subDirFs;
    }

    private (IFileSystem baseFs, IFileSystem subDirFs) CreateFileSystemInternal()
    {
        var baseFs = new InMemoryFileSystem();
        baseFs.CreateDirectory("/sub");
        baseFs.CreateDirectory("/sub/path");

        using var rootPath = new Path();
        PathFunctions.SetUpFixedPath(ref rootPath.Ref(), "/sub/path"u8);

        var subFs = new SubdirectoryFileSystem(baseFs);
        subFs.Initialize(in rootPath).ThrowIfFailure();

        return (baseFs, subFs);
    }

    [Test]
    public void CreateFile_CreatedInBaseFileSystem()
    {
        (IFileSystem baseFs, IFileSystem subDirFs) = CreateFileSystemInternal();

        subDirFs.CreateFile("/file", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Success(baseFs.GetEntryType(out DirectoryEntryType type, "/sub/path/file"));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void CreateDirectory_CreatedInBaseFileSystem()
    {
        (IFileSystem baseFs, IFileSystem subDirFs) = CreateFileSystemInternal();

        subDirFs.CreateDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Success(baseFs.GetEntryType(out DirectoryEntryType type, "/sub/path/dir"));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.Directory));
    }
}

public class SubdirectoryFileSystemTestsRoot : IFileSystemTests
{
    protected override IFileSystem CreateFileSystem()
    {
        var baseFs = new InMemoryFileSystem();

        using var rootPath = new Path();
        PathFunctions.SetUpFixedPath(ref rootPath.Ref(), "/"u8);

        var subFs = new SubdirectoryFileSystem(baseFs);
        subFs.Initialize(in rootPath).ThrowIfFailure();
        return subFs;
    }
}