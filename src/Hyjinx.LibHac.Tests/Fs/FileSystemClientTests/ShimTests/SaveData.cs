using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;
using NUnit.Framework;

namespace LibHac.Tests.Fs.FileSystemClientTests.ShimTests;

public class SaveData
{
    [Test]
    public void MountCacheStorage_CanMountCreatedCacheStorage()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, SaveDataFlags.None);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountCacheStorage("cache"u8, applicationId));
    }

    [Test]
    public void MountCacheStorage_WrittenDataPersists()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        fs.CreateCacheStorage(applicationId, SaveDataSpaceId.SdUser, applicationId.Value, 0, 0, SaveDataFlags.None);
        fs.MountCacheStorage("cache"u8, applicationId);

        fs.CreateFile("cache:/file"u8, 0);
        fs.Commit("cache"u8);
        fs.Unmount("cache"u8);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountCacheStorage("cache"u8, applicationId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "cache:/file"u8));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void MountCacheStorage_SdCardIsPreferredOverBis()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateCacheStorage(applicationId, SaveDataSpaceId.SdUser, applicationId.Value, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountCacheStorage("cache"u8, applicationId));
        fs.CreateFile("cache:/sd"u8, 0);
        fs.Commit("cache"u8);
        fs.Unmount("cache"u8);

        // Turn off the SD card so the User save is mounted
        fs.SetSdCardAccessibility(false);

        fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, SaveDataFlags.None);
        fs.MountCacheStorage("cache"u8, applicationId);
        fs.CreateFile("cache:/bis"u8, 0);
        fs.Commit("cache"u8);
        fs.Unmount("cache"u8);

        fs.SetSdCardAccessibility(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountCacheStorage("cache"u8, applicationId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out _, "cache:/sd"u8));
        LibHac.Tests.Fs.FsTestAssert.Failure(fs.GetEntryType(out _, "cache:/bis"u8));
    }
}