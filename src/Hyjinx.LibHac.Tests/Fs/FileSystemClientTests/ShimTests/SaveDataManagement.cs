using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSrv.Impl;
using LibHac.Ncm;
using LibHac.Time;
using System;
using System.Linq;
using NUnit.Framework;
using static LibHac.Fs.SaveData;

namespace LibHac.Tests.Fs.FileSystemClientTests.ShimTests;

public class SaveDataManagement
{
    [Test]
    public void CreateCacheStorage_InUserSaveSpace_StorageIsCreated()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, SaveDataFlags.None));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));
    }

    [Test]
    public void CreateCacheStorage_InSdCacheSaveSpace_StorageIsCreated()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateCacheStorage(applicationId, SaveDataSpaceId.SdUser, applicationId.Value, 0, 0, SaveDataFlags.None));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.SdUser);

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));
    }

    [Test]
    public void CreateCacheStorage_InSdCacheSaveSpaceWhenNoSdCard_ReturnsSdCardNotFound()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(false);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PortSdCardNoDevice, fs.CreateCacheStorage(applicationId, SaveDataSpaceId.SdUser, applicationId.Value, 0, 0, SaveDataFlags.None));
    }

    [Test]
    public void CreateCacheStorage_AlreadyExists_ReturnsPathAlreadyExists()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, SaveDataFlags.None));
    }

    [Test]
    public void CreateCacheStorage_WithIndex_CreatesMultiple()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 1, 0, 0, SaveDataFlags.None));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

        var info = new SaveDataInfo[3];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(2));
        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));
        Assert.That(info[1].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));

        ushort[] expectedIndexes = { 0, 1 };
        ushort[] actualIndexes = info.Take(2).Select(x => x.Index).OrderBy(x => x).ToArray();

        Assert.That(actualIndexes, Is.EqualTo(expectedIndexes));
    }

    [Test]
    public void CreateBcatSaveData_DoesNotExist_SaveIsCreated()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateBcatSaveData(applicationId, 0x400000));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User);

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Bcat));
    }

    [Test]
    public void CreateBcatSaveData_AlreadyExists_ReturnsPathAlreadyExists()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateBcatSaveData(applicationId, 0x400000));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, fs.CreateBcatSaveData(applicationId, 0x400000));
    }

    [Test]
    public void CreateSystemSaveData_DoesNotExist_SaveIsCreatedInSystem()
    {
        ulong saveId = 0x8000000001234000;

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSystemSaveData(saveId, 0x1000, 0x1000, SaveDataFlags.None));

        // Make sure it was placed in the System save space with the right info.
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.System));

        var info = new SaveDataInfo[2];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.System));
        Assert.That(info[0].SpaceId, Is.EqualTo(SaveDataSpaceId.System));
        Assert.That(info[0].StaticSaveDataId, Is.EqualTo(saveId));
        Assert.That(info[0].SaveDataId, Is.EqualTo(saveId));
        Assert.That(info[0].State, Is.EqualTo(SaveDataState.Normal));
    }
    [TestCase(AccessControlBits.Bits.SystemSaveData)]
    [TestCase(AccessControlBits.Bits.None)]
    public void CreateSystemSaveData_HasBuiltInSystemPermission_SaveIsCreatedInSystem(AccessControlBits.Bits permissions)
    {
        ulong saveId = 0x8000000001234000;

        Horizon hos = FileSystemServerFactory.CreateHorizonServer();

        var mainProgramId = new ProgramId(0x123456);

        HorizonClient client = hos.CreateHorizonClient(new ProgramLocation(mainProgramId, StorageId.BuiltInSystem),
            permissions);

        HorizonClient privilegedClient = hos.CreatePrivilegedHorizonClient();

        // Create the save
        if (permissions.HasFlag(AccessControlBits.Bits.SystemSaveData))
        {
            LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateSystemSaveData(saveId, 0x1000, 0x1000, SaveDataFlags.None));
        }
        else
        {
            // Creation should fail if we don't have the right permissions.
            LibHac.Tests.Fs.FsTestAssert.Failure(client.Fs.CreateSystemSaveData(saveId, 0x1000, 0x1000, SaveDataFlags.None));
            return;
        }

        // Make sure it was placed in the System save space with the right info.
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(privilegedClient.Fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.System));

        var info = new SaveDataInfo[2];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long entriesRead, info));

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.System));
        Assert.That(info[0].SpaceId, Is.EqualTo(SaveDataSpaceId.System));
        Assert.That(info[0].StaticSaveDataId, Is.EqualTo(saveId));
        Assert.That(info[0].SaveDataId, Is.EqualTo(saveId));
        Assert.That(info[0].State, Is.EqualTo(SaveDataState.Normal));
    }

    [Test]
    public void CreateSaveData_DoesNotExist_SaveIsCreated()
    {
        var applicationId = new Ncm.ApplicationId(1);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, 0, 0x1000, 0x1000, SaveDataFlags.None));

        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Account));
        Assert.That(info[0].UserId, Is.EqualTo(userId));
    }

    [Test]
    public void CreateSaveData_DoesNotExist_HasCorrectOwnerId()
    {
        uint ownerId = 1;

        var applicationId = new Ncm.ApplicationId(ownerId);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, ownerId, 0x1000, 0x1000, SaveDataFlags.None));

        // Get the created save data's ID
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));

        // Get the created save data's owner ID
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataOwnerId(out ulong actualOwnerId, info[0].SaveDataId));

        Assert.That(actualOwnerId, Is.EqualTo(ownerId));
    }

    [Test]
    public void CreateSaveData_DoesNotExist_HasCorrectFlags()
    {
        SaveDataFlags flags = SaveDataFlags.KeepAfterRefurbishment | SaveDataFlags.NeedsSecureDelete;

        var applicationId = new Ncm.ApplicationId(1);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, 0, 0x1000, 0x1000, flags));

        // Get the created save data's ID
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));

        // Get the created save data's flags
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataFlags(out SaveDataFlags actualFlags, info[0].SaveDataId));

        Assert.That(actualFlags, Is.EqualTo(flags));
    }

    [Test]
    public void CreateSaveData_DoesNotExist_HasCorrectSizes()
    {
        long availableSize = 0x220000;
        long journalSize = 0x120000;

        var applicationId = new Ncm.ApplicationId(1);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, 0, availableSize, journalSize, SaveDataFlags.None));

        // Get the created save data's ID
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));

        // Get the created save data's sizes
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataAvailableSize(out long actualAvailableSize, info[0].SaveDataId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataJournalSize(out long actualJournalSize, info[0].SaveDataId));

        Assert.That(actualAvailableSize, Is.EqualTo(availableSize));
        Assert.That(actualJournalSize, Is.EqualTo(journalSize));
    }

    [Test]
    public void CreateSaveData_FromSubProgram_CreatesSaveDataForMainProgram()
    {
        Horizon hos = FileSystemServerFactory.CreateHorizonServer();

        Span<ProgramIndexMapInfo> mapInfo = stackalloc ProgramIndexMapInfo[5];

        var mainProgramId = new Ncm.ApplicationId(0x123456);
        var programId = new ProgramId(mainProgramId.Value + 2);

        for (int i = 0; i < mapInfo.Length; i++)
        {
            mapInfo[i].MainProgramId = mainProgramId;
            mapInfo[i].ProgramId = new ProgramId(mainProgramId.Value + (uint)i);
            mapInfo[i].ProgramIndex = (byte)i;
        }

        HorizonClient client = hos.CreatePrivilegedHorizonClient();
        HorizonClient subProgramClient =
            hos.CreateHorizonClient(new ProgramLocation(programId, StorageId.BuiltInUser),
                AccessControlBits.Bits.CreateSaveData);

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.RegisterProgramIndexMapInfo(mapInfo));

        LibHac.Tests.Fs.FsTestAssert.Success(subProgramClient.Fs.CreateSaveData(Ncm.ApplicationId.InvalidId, InvalidUserId, 0, 0x4000,
            0x4000, SaveDataFlags.None));

        // Get the created save data's ID
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));

        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)mainProgramId));
    }

    [Test]
    public void CreateTemporaryStorage_DoesNotExist_SaveIsCreated()
    {
        long availableSize = 0x220000;

        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateTemporaryStorage(applicationId, 0, availableSize, SaveDataFlags.None));

        using var iterator = new UniqueRef<SaveDataIterator>();
        fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.Temporary);

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));
        Assert.That(info[0].ProgramId, Is.EqualTo((Ncm.ProgramId)applicationId));
        Assert.That(info[0].Type, Is.EqualTo(SaveDataType.Temporary));
    }

    [Test]
    public void CreateTemporaryStorage_AlreadyExists_ReturnsPathAlreadyExists()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateTemporaryStorage(applicationId, 0, 0x400000, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathAlreadyExists, fs.CreateTemporaryStorage(applicationId, 0, 0x400000, SaveDataFlags.None));
    }

    [Test]
    public void DeleteSaveData_DoesNotExist_ReturnsTargetNotFound()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.TargetNotFound, fs.DeleteSaveData(1));
    }

    [Test]
    public void DeleteSaveData_SaveExistsInUserSaveSpace_SaveIsDeleted()
    {
        var applicationId = new Ncm.ApplicationId(1);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, 0, 0x1000, 0x1000, SaveDataFlags.None));

        // Get the ID of the save
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[1];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out _, info));

        // Delete the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.DeleteSaveData(info[0].SaveDataId));

        // Iterate saves again
        using var iterator2 = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator2.Ref, SaveDataSpaceId.User));
        LibHac.Tests.Fs.FsTestAssert.Success(iterator2.Get.ReadSaveDataInfo(out long entriesRead, info));

        // Make sure no saves were returned
        Assert.That(entriesRead, Is.EqualTo(0));
    }
    [TestCase((int)(2), (int)(3), (int)(2))]
    [TestCase((int)(3), (int)(3), (int)(4))]
    [TestCase((int)(5), (int)(3), (int)(5))]
    public void DeleteSaveData_SaveDataIteratorsAreOpen_IteratorsPointToSameEntry(int nextEntryWhenRemoving,
        int entryToRemove, int expectedNextEntry)
    {
        const int count = 20;

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create saves
        LibHac.Tests.Fs.FsTestAssert.Success(PopulateSaveData(fs, count));

        // Open an iterator
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        // Skip ahead a few entries. Entries start counting at 1, so subtract 1
        var infos = new SaveDataInfo[nextEntryWhenRemoving - 1];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCount, infos));
        Assert.That(readCount, Is.EqualTo(infos.Length));

        // Delete the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.DeleteSaveData(SaveDataSpaceId.User, (ulong)entryToRemove));

        // Check the program ID of the next entry
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCount2, infos.AsSpan(0, 1)));
        Assert.That(readCount2, Is.EqualTo(1));

        Assert.That(infos[0].ProgramId.Value, Is.EqualTo((ulong)expectedNextEntry));
    }
    [TestCase((int)(6), (int)(7), (int)(6))]
    [TestCase((int)(8), (int)(7), (int)(8))]
    public void CreateSaveData_SaveDataIteratorsAreOpen_IteratorsPointToSameEntry(int nextEntryWhenAdding,
        int entryToAdd, int expectedNextEntry)
    {
        // Static save IDs must have the high bit set
        const ulong mask = 0x8000000000000000;
        const int count = 10;

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create saves
        for (int i = 0; i < count * 2; i++)
        {
            LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSystemSaveData(SaveDataSpaceId.User, ((ulong)i * 2) | mask, 0, 0x4000,
                0x4000, SaveDataFlags.None));
        }

        // Open an iterator
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        // Skip ahead a few entries. We skipped 0 and added every other ID, so divide by 2 and subtract 1
        var infos = new SaveDataInfo[nextEntryWhenAdding / 2 - 1];
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCount, infos));
        Assert.That(readCount, Is.EqualTo(infos.Length));

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSystemSaveData(SaveDataSpaceId.User, (uint)entryToAdd | mask, (ulong)entryToAdd, 0x4000,
            0x4000, SaveDataFlags.None));

        // Check the save ID of the next entry
        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCount2, infos.AsSpan(0, 1)));
        Assert.That(readCount2, Is.EqualTo(1));

        Assert.That(infos[0].SaveDataId, Is.EqualTo((uint)expectedNextEntry | mask));
    }

    [Test]
    public void OpenSaveDataIterator_MultipleSavesExist_IteratorReturnsSavesInOrder()
    {
        const int count = 20;
        const int rngSeed = 359;

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(PopulateSaveData(fs, count, rngSeed));

        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo();
        for (int i = 0; i < count; i++)
        {
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCount, SpanHelpers.AsSpan(ref info)));

            Assert.That(readCount, Is.EqualTo(1));
            Assert.That(info.ProgramId.Value, Is.EqualTo((ulong)i));
        }

        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCountFinal, SpanHelpers.AsSpan(ref info)));

        Assert.That(readCountFinal, Is.EqualTo(0));
    }

    [Test]
    public void ReadSaveDataInfo_WhenFilteringSavesByUserId_IteratorReturnsAllMatchingSaves()
    {
        const int count = 10;
        const int countUser1 = 5;

        var user1Id = new UserId(0x1234, 0x5678);
        var user2Id = new UserId(0x1122, 0x3344);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        for (int i = 1; i <= countUser1; i++)
        {
            var applicationId = new Ncm.ApplicationId((uint)i);
            LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, user1Id, 0, 0x4000, 0x4000, SaveDataFlags.None));
        }

        for (int i = countUser1 + 1; i <= count; i++)
        {
            var applicationId = new Ncm.ApplicationId((uint)i);
            LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, user2Id, 0, 0x4000, 0x4000, SaveDataFlags.None));
        }

        LibHac.Tests.Fs.FsTestAssert.Success(SaveDataFilter.Make(out SaveDataFilter filter, programId: default, saveType: default, user2Id,
            saveDataId: default, index: default));

        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User, in filter));

        var info = new SaveDataInfo();
        for (int i = countUser1 + 1; i <= count; i++)
        {
            LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCount, SpanHelpers.AsSpan(ref info)));

            Assert.That(readCount, Is.EqualTo(1));
            Assert.That(info.ProgramId.Value, Is.EqualTo((ulong)i));
            Assert.That(info.UserId, Is.EqualTo(user2Id));
        }

        LibHac.Tests.Fs.FsTestAssert.Success(iterator.Get.ReadSaveDataInfo(out long readCountFinal, SpanHelpers.AsSpan(ref info)));

        Assert.That(readCountFinal, Is.EqualTo(0));
    }

    [Test]
    public void GetSaveDataCommitId_AfterSetSaveDataCommitIdIsCalled_ReturnsSetCommitId()
    {
        long commitId = 46506854;

        var applicationId = new Ncm.ApplicationId(1);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, 0, 0x1000, 0x1000, SaveDataFlags.None));

        // Get the created save data's ID
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));

        // Set the new commit ID
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetSaveDataCommitId(info[0].SpaceId, info[0].SaveDataId, commitId));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataCommitId(out long actualCommitId, info[0].SpaceId, info[0].SaveDataId));

        Assert.That(actualCommitId, Is.EqualTo(commitId));
    }

    [Test]
    public void GetSaveDataTimeStamp_AfterSetSaveDataTimeStampIsCalled_ReturnsSetTimeStamp()
    {
        var timeStamp = new PosixTime(12345678);

        var applicationId = new Ncm.ApplicationId(1);
        var userId = new UserId(5, 4);

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        // Create the save
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateSaveData(applicationId, userId, 0, 0x1000, 0x1000, SaveDataFlags.None));

        // Get the created save data's ID
        using var iterator = new UniqueRef<SaveDataIterator>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenSaveDataIterator(ref iterator.Ref, SaveDataSpaceId.User));

        var info = new SaveDataInfo[2];
        iterator.Get.ReadSaveDataInfo(out long entriesRead, info);

        Assert.That(entriesRead, Is.EqualTo(1));

        // Set the new timestamp
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetSaveDataTimeStamp(info[0].SpaceId, info[0].SaveDataId, timeStamp));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetSaveDataTimeStamp(out PosixTime actualTimeStamp, info[0].SpaceId, info[0].SaveDataId));

        Assert.That(actualTimeStamp, Is.EqualTo(timeStamp));
    }

    [Test]
    public void OpenCacheStorageList_ReadIntoLargeBuffer_AllIndexesAreRead()
    {
        var applicationId = new Ncm.ApplicationId(1);

        Horizon hos = HorizonFactory.CreateBasicHorizon();

        HorizonClient client = hos.CreateHorizonClient(new ProgramLocation(applicationId, StorageId.BuiltInSystem),
            (AccessControlBits.Bits)ulong.MaxValue);

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 6, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 2, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 3, 0, 0, SaveDataFlags.None));

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.OpenCacheStorageList(out CacheStorageListHandle handle));
        var infoBuffer = new CacheStorageInfo[5];

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.ReadCacheStorageList(out int readCount, infoBuffer, handle));

        Assert.That(readCount, Is.EqualTo(4));

        Assert.That(infoBuffer[0].Index, Is.EqualTo(0));
        Assert.That(infoBuffer[1].Index, Is.EqualTo(2));
        Assert.That(infoBuffer[2].Index, Is.EqualTo(3));
        Assert.That(infoBuffer[3].Index, Is.EqualTo(6));

        client.Fs.CloseCacheStorageList(handle);
    }

    [Test]
    public void OpenCacheStorageList_ReadIntoMultipleBuffers_AllIndexesAreRead()
    {
        var applicationId = new Ncm.ApplicationId(1);

        Horizon hos = HorizonFactory.CreateBasicHorizon();

        HorizonClient client = hos.CreateHorizonClient(new ProgramLocation(applicationId, StorageId.BuiltInSystem),
            (AccessControlBits.Bits)ulong.MaxValue);

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 0, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 6, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 2, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.CreateCacheStorage(applicationId, SaveDataSpaceId.User, applicationId.Value, 3, 0, 0, SaveDataFlags.None));

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.OpenCacheStorageList(out CacheStorageListHandle handle));
        var infoBuffer = new CacheStorageInfo[2];

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.ReadCacheStorageList(out int readCount, infoBuffer, handle));

        Assert.That(readCount, Is.EqualTo(2));
        Assert.That(infoBuffer[0].Index, Is.EqualTo(0));
        Assert.That(infoBuffer[1].Index, Is.EqualTo(2));

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.ReadCacheStorageList(out readCount, infoBuffer, handle));

        Assert.That(readCount, Is.EqualTo(2));
        Assert.That(infoBuffer[0].Index, Is.EqualTo(3));
        Assert.That(infoBuffer[1].Index, Is.EqualTo(6));

        LibHac.Tests.Fs.FsTestAssert.Success(client.Fs.ReadCacheStorageList(out readCount, infoBuffer, handle));

        Assert.That(readCount, Is.EqualTo(0));

        client.Fs.CloseCacheStorageList(handle);
    }

    private static Result PopulateSaveData(FileSystemClient fs, int count, int seed = -1)
    {
        if (seed == -1)
        {
            for (int i = 1; i <= count; i++)
            {
                var applicationId = new Ncm.ApplicationId((uint)i);
                Result res = fs.CreateSaveData(applicationId, InvalidUserId, 0, 0x4000, 0x4000, SaveDataFlags.None);
                if (res.IsFailure())
                    return res.Miss();
            }
        }
        else
        {
            var rng = new FullCycleRandom(count, seed);

            for (int i = 1; i <= count; i++)
            {
                var applicationId = new Ncm.ApplicationId((uint)rng.Next());
                Result res = fs.CreateSaveData(applicationId, InvalidUserId, 0, 0x4000, 0x4000, SaveDataFlags.None);
                if (res.IsFailure())
                    return res.Miss();
            }
        }

        return Result.Success;
    }
}