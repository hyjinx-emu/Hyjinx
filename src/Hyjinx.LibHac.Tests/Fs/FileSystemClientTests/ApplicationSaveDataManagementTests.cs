using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.Ns;
using static LibHac.Fs.SaveData;

namespace LibHac.Tests.Fs.FileSystemClientTests;

public class ApplicationSaveDataManagementTests
{
    [Test]
    public static void EnsureApplicationSaveData_CreatesAccountSaveData()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var applicationId = new Ncm.ApplicationId(11);
        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            UserAccountSaveDataSize = 0x1000,
            UserAccountSaveDataJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        // EnsureApplicationSaveData should do nothing the second time it is called
        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

            var info = new SaveDataInfo[2];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(1));
            Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
            Assert.That(info[0].UserId, Is.EqualTo(Utility.ConvertAccountUidToFsUserId(userId)));
            Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Account));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_CreatesMultipleAccountSaveData_AllSavesExist()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            UserAccountSaveDataSize = 0x1000,
            UserAccountSaveDataJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, new Ncm.ApplicationId(11), in controlProperty, in userId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, new Ncm.ApplicationId(12), in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

            var info = new SaveDataInfo[3];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(2));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_CreatesDeviceSaveData()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var applicationId = new Ncm.ApplicationId(11);
        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            DeviceSaveDataSize = 0x1000,
            DeviceSaveDataJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

            var info = new SaveDataInfo[2];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(1));
            Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
            Assert.That(info[0].UserId, Is.EqualTo(InvalidUserId));
            Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Device));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_CreatesMultipleDeviceSaveData_AllSavesExist()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            DeviceSaveDataSize = 0x1000,
            DeviceSaveDataJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, new Ncm.ApplicationId(11), in controlProperty, in userId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, new Ncm.ApplicationId(12), in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

            var info = new SaveDataInfo[3];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(2));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_CreatesBcatCacheStorage()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var applicationId = new Ncm.ApplicationId(11);
        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            BcatDeliveryCacheStorageSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

            var info = new SaveDataInfo[2];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(1));
            Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
            Assert.That(info[0].UserId, Is.EqualTo(InvalidUserId));
            Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Bcat));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_CreatesMultipleBcatCacheStorages_AllSavesExist()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            BcatDeliveryCacheStorageSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, new Ncm.ApplicationId(11), in controlProperty, in userId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, new Ncm.ApplicationId(12), in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

            var info = new SaveDataInfo[3];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(2));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_CreatesTemporaryStorage()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var applicationId = new Ncm.ApplicationId(11);
        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            TemporaryStorageSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));
        AssertSaveExists();

        void AssertSaveExists()
        {
            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.Temporary);

            var info = new SaveDataInfo[2];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(1));
            Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
            Assert.That(info[0].UserId, Is.EqualTo(InvalidUserId));
            Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Temporary));
        }
    }

    [Test]
    public static void EnsureApplicationSaveData_NeedsExtension_IsExtended()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var applicationId = new Ncm.ApplicationId(11);
        var userId = new Uid(2, 3);

        var controlProperty = new ApplicationControlProperty
        {
            UserAccountSaveDataSize = 0x1000,
            UserAccountSaveDataJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));

        const int newAvailableSize = 1024 * 1024 * 2;
        const int newJournalSize = 1024 * 1024;

        controlProperty.UserAccountSaveDataSize = newAvailableSize;
        controlProperty.UserAccountSaveDataJournalSize = newJournalSize;

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationSaveData(out _, applicationId, in controlProperty, in userId));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

        var info = new SaveDataInfo[2];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
        Assert.That(info[0].UserId, Is.EqualTo(Utility.ConvertAccountUidToFsUserId(userId)));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Account));

        // ReSharper disable UnusedVariable
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataAvailableSize(out long availableSize, SaveDataSpaceId.User, info[0].SaveDataId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataJournalSize(out long journalSize, SaveDataSpaceId.User, info[0].SaveDataId));
        // ReSharper restore UnusedVariable

        // Todo: Remove once save data extension is implemented
        // Assert.Equal(newAvailableSize, availableSize);
        // Assert.Equal(newJournalSize, journalSize);
    }

    [Test]
    public static void EnsureApplicationCacheStorage_SdCardAvailable_CreatesCacheStorageOnSd()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        var applicationId = new Ncm.ApplicationId(11);

        var controlProperty = new ApplicationControlProperty
        {
            CacheStorageSize = 0x1000,
            CacheStorageJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationCacheStorage(out _, out CacheStorageTargetMedia target, applicationId,
            in controlProperty));
        AssertSaveExists();

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationCacheStorage(out _, out target, applicationId, in controlProperty));
        AssertSaveExists();

        void AssertSaveExists()
        {
            Assert.That(target, Is.EqualTo(CacheStorageTargetMedia.SdCard));

            using var iterator = new UniqueRef<SaveDataIterator>();
            fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.SdUser);

            var info = new SaveDataInfo[2];
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

            Assert.That(entriesRead, Is.EqualTo(1));
            Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
            Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Cache));
        }
    }

    [Test]
    public static void EnsureApplicationCacheStorage_SdCardNotAvailable_CreatesCacheStorageOnBis()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(false);

        var applicationId = new Ncm.ApplicationId(11);

        var controlProperty = new ApplicationControlProperty
        {
            CacheStorageSize = 0x1000,
            CacheStorageJournalSize = 0x1000
        };

        LibHac.Tests.Fs.FsTestAssert.Success(fs.EnsureApplicationCacheStorage(out _, out CacheStorageTargetMedia target, applicationId,
            in controlProperty));

        Assert.That(target, Is.EqualTo(CacheStorageTargetMedia.Nand));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

        var info = new SaveDataInfo[2];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((LibHac.Ncm.ProgramId)applicationId));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Cache));
    }
    [TestCase(true)]
    [TestCase(false)]
    public static void GetCacheStorageTargetMedia_ReturnsTargetOfNewCacheStorage(bool isSdCardInserted)
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(isSdCardInserted);

        var applicationId = new Ncm.ApplicationId(11);

        var controlProperty = new ApplicationControlProperty
        {
            CacheStorageSize = 0x1000,
            CacheStorageJournalSize = 0x1000
        };

        fs.EnsureApplicationCacheStorage(out _, out CacheStorageTargetMedia targetFromCreation, applicationId,
            in controlProperty);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetCacheStorageTargetMedia(out CacheStorageTargetMedia target, applicationId));
        Assert.That(target, Is.EqualTo(targetFromCreation));
    }

    [Test]
    public static void GetCacheStorageTargetMedia_CacheStorageDoesNotExist_ReturnsNone()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetCacheStorageTargetMedia(out CacheStorageTargetMedia target, new Ncm.ApplicationId(11)));
        Assert.That(target, Is.EqualTo(CacheStorageTargetMedia.None));
    }
}