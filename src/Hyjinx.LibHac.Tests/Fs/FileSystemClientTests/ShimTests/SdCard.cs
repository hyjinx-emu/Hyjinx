using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;

namespace LibHac.Tests.Fs.FileSystemClientTests.ShimTests;

public class SdCard
{
    [Test]
    public void MountSdCard_CardIsInserted_Succeeds()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountSdCard("sdcard"u8));
    }

    [Test]
    public void MountSdCard_CardIsNotInserted_Fails()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(false);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PortSdCardNoDevice, fs.MountSdCard("sdcard"u8));
    }

    [Test]
    public void MountSdCard_CanWriteToFsAfterMounted()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        fs.MountSdCard("sdcard"u8);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateFile("sdcard:/file"u8, 100, CreateFileOptions.None));
    }

    [Test]
    public void IsSdCardInserted_CardIsInserted_ReturnsTrue()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        Assert.That(fs.IsSdCardInserted(), Is.True);
    }

    [Test]
    public void IsSdCardInserted_CardIsNotInserted_ReturnsFalse()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(false);

        Assert.That(fs.IsSdCardInserted(), Is.False);
    }

    [Test]
    public void IsSdCardAccessible_CardIsInserted_ReturnsTrue()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        Assert.That(fs.IsSdCardAccessible(), Is.True);
    }

    [Test]
    public void IsSdCardAccessible_CardIsNotInserted_ReturnsFalse()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(false);

        Assert.That(fs.IsSdCardAccessible(), Is.False);
    }

    [Test]
    public void SetSdCardAccessibility_SetAccessibilityPersists()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(false);

        fs.SetSdCardAccessibility(true);
        Assert.That(fs.IsSdCardAccessible(), Is.True);

        fs.SetSdCardAccessibility(false);
        Assert.That(fs.IsSdCardAccessible(), Is.False);
    }
}