using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void IFileWrite_CanReadBackWrittenData()
    {
        byte[] data = { 7, 4, 1, 0, 8, 5, 2, 9, 6, 3 };

        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", data.Length, CreateFileOptions.None);

        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Write);
        file.Get.Write(0, data, WriteOption.None);
        file.Reset();

        byte[] readData = new byte[data.Length];
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Read);

        LibHac.Tests.Fs.FsTestAssert.Success(file.Get.Read(out long bytesRead, 0, readData, ReadOption.None));
        Assert.That(bytesRead, Is.EqualTo(data.Length));

        Assert.That(readData, Is.EqualTo(data));
    }

    [Test]
    public void IFileWrite_WritePastEndOfFileWithNoAppend_ReturnsFileExtensionWithoutOpenModeAllowAppend()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] buffer = new byte[10];
        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Write);

        Result res = file.Get.Write(5, buffer, WriteOption.None);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.FileExtensionWithoutOpenModeAllowAppend, res);
    }

    [Test]
    public void IFileWrite_OpenModeNoWrite_ReturnsInvalidOpenModeForWrite()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] buffer = new byte[10];
        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Read);

        Result res = file.Get.Write(5, buffer, WriteOption.None);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.WriteUnpermitted, res);
    }

    [Test]
    public void IFileWrite_NegativeOffset_ReturnsOutOfRange()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] buffer = new byte[10];
        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Read);

        Result res = file.Get.Write(-5, buffer, WriteOption.None);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.OutOfRange, res);
    }

    [Test]
    public void IFileWrite_OffsetPlusSizeOverflows_ReturnsOutOfRange()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] buffer = new byte[10];
        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.Read);

        Result res = file.Get.Write(long.MaxValue - 5, buffer, WriteOption.None);
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.OutOfRange, res);
    }

    [Test]
    public void IFileWrite_WritePartiallyPastEndOfFileAppendAllowed_FileIsExtended()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] buffer = new byte[10];
        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.All);

        LibHac.Tests.Fs.FsTestAssert.Success(file.Get.Write(5, buffer, WriteOption.None));

        file.Get.GetSize(out long newSize);
        Assert.That(newSize, Is.EqualTo(15));
    }

    [Test]
    public void IFileWrite_WritePastEndOfFileAppendAllowed_FileIsExtended()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] buffer = new byte[10];
        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.All);

        LibHac.Tests.Fs.FsTestAssert.Success(file.Get.Write(15, buffer, WriteOption.None));

        file.Get.GetSize(out long newSize);
        Assert.That(newSize, Is.EqualTo(25));
    }

    [Test]
    public void IFileWrite_WritePastEndOfFileAppendAllowed_DataIsWritten()
    {
        IFileSystem fs = CreateFileSystem();

        fs.CreateFile("/file", 10, CreateFileOptions.None);

        byte[] bufferExpected = new byte[25];
        bufferExpected.AsSpan(15).Fill(0xCC);

        byte[] writeBuffer = new byte[10];
        writeBuffer.AsSpan().Fill(0xCC);

        using var file = new UniqueRef<IFile>();
        fs.OpenFile(ref file.Ref, "/file", OpenMode.All);

        LibHac.Tests.Fs.FsTestAssert.Success(file.Get.Write(15, writeBuffer, WriteOption.None));

        // Unwritten portions of new files are undefined, so write to the other portions
        file.Get.Write(0, new byte[15], WriteOption.None);
        file.Reset();

        byte[] readBuffer = new byte[25];

        fs.OpenFile(ref file.Ref, "/file", OpenMode.Read);

        file.Get.Read(out _, 0, readBuffer, ReadOption.None);
        Assert.That(readBuffer, Is.EqualTo(bufferExpected));
    }
}