using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using NUnit.Framework;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void RenameFile_SameParentDirectory_EntryIsRenamed()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file1", 0, CreateFileOptions.None);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.RenameFile("/file1", "/file2"));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "/file2"));
        Result res = fs.GetEntryType(out _, "/file1");

        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void RenameFile_DifferentParentDirectory_EntryIsRenamed()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file1", 0, CreateFileOptions.None);
        fs.CreateDirectory("/dir");

        LibHac.Tests.Fs.FsTestAssert.Success(fs.RenameFile("/file1", "/dir/file2"));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "/dir/file2"));
        Result res = fs.GetEntryType(out _, "/file1");

        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, res);
    }

    [Test]
    public void RenameFile_DestExistsAsFile_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file1", 0, CreateFileOptions.None);
        fs.CreateFile("/file2", 0, CreateFileOptions.None);

        Result res = fs.RenameFile("/file1", "/file2");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, res);
    }

    [Test]
    public void RenameFile_DestExistsAsDirectory_ReturnsPathAlreadyExists()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 0, CreateFileOptions.None);
        fs.CreateDirectory("/dir");

        Result res = fs.RenameFile("/file", "/dir");

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, res);
    }

    [Test]
    public void RenameFile_DestExistsAsFile_FileSizesDoNotChange()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file1", 54321, CreateFileOptions.None);
        fs.CreateFile("/file2", 12345, CreateFileOptions.None);

        fs.RenameFile("/file1", "/file2");

        using var file1 = new UniqueRef<IFile>();
        using var file2 = new UniqueRef<IFile>();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenFile(ref file1.Ref, "/file1", OpenMode.Read));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenFile(ref file2.Ref, "/file2", OpenMode.Read));

        LibHac.Tests.Fs.FsTestAssert.Success(file1.Get.GetSize(out long file1Size));
        LibHac.Tests.Fs.FsTestAssert.Success(file2.Get.GetSize(out long file2Size));

        Assert.That(file1Size, Is.EqualTo(54321));
        Assert.That(file2Size, Is.EqualTo(12345));
    }

    [Test]
    public void RenameFile_DataIsUnmodified()
    {
        byte[] data = { 7, 4, 1, 0, 8, 5, 2, 9, 6, 3 };

        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", data.Length, CreateFileOptions.None);

        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Write);
        file.Get.Write(0, data, WriteOption.None);
        file.Reset();

        fs.RenameFile("/file", "/renamed");

        byte[] readData = new byte[data.Length];

        fs.OpenFile(ref file.Ref, "/renamed", OpenMode.Read);
        Result res = file.Get.Read(out long bytesRead, 0, readData, ReadOption.None);

        LibHac.Tests.Fs.FsTestAssert.Success(res);
        Assert.That(bytesRead, Is.EqualTo(data.Length));
        Assert.That(readData, Is.EqualTo(data));
    }
}