using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Impl;
using System.Runtime.CompilerServices;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Fs;

public class TypeLayoutTests
{
    [Test]
    public static void SaveDataAttribute_Layout()
    {
        var s = new SaveDataAttribute();

        Assert.That(Unsafe.SizeOf<SaveDataAttribute>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.ProgramId), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.UserId), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.StaticSaveDataId), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.Type), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.Rank), Is.EqualTo(0x21));
        Assert.That(GetOffset(in s, in s.Index), Is.EqualTo(0x22));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x24));
    }

    [Test]
    public static void SaveDataCreationInfo_Layout()
    {
        var s = new SaveDataCreationInfo();

        Assert.That(Unsafe.SizeOf<SaveDataCreationInfo>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.JournalSize), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.BlockSize), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.OwnerId), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.SpaceId), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.IsPseudoSaveData), Is.EqualTo(0x25));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x26));
    }

    [Test]
    public static void SaveDataCreationInfo2_Layout()
    {
        var s = new SaveDataCreationInfo2();

        Assert.That(Unsafe.SizeOf<SaveDataCreationInfo2>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Attribute), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.JournalSize), Is.EqualTo(0x50));
        Assert.That(GetOffset(in s, in s.BlockSize), Is.EqualTo(0x58));
        Assert.That(GetOffset(in s, in s.OwnerId), Is.EqualTo(0x60));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x68));
        Assert.That(GetOffset(in s, in s.SpaceId), Is.EqualTo(0x6C));
        Assert.That(GetOffset(in s, in s.FormatType), Is.EqualTo(0x6D));
        Assert.That(GetOffset(in s, in s.Reserved1), Is.EqualTo(0x6E));
        Assert.That(GetOffset(in s, in s.IsHashSaltEnabled), Is.EqualTo(0x70));
        Assert.That(GetOffset(in s, in s.Reserved2), Is.EqualTo(0x71));
        Assert.That(GetOffset(in s, in s.HashSalt), Is.EqualTo(0x74));
        Assert.That(GetOffset(in s, in s.MetaType), Is.EqualTo(0x94));
        Assert.That(GetOffset(in s, in s.Reserved3), Is.EqualTo(0x95));
        Assert.That(GetOffset(in s, in s.MetaSize), Is.EqualTo(0x98));
        Assert.That(GetOffset(in s, in s.Reserved4), Is.EqualTo(0x9C));
    }

    [Test]
    public static void SaveDataFilter_Layout()
    {
        var s = new SaveDataFilter();

        Assert.That(Unsafe.SizeOf<SaveDataFilter>(), Is.EqualTo(0x48));

        Assert.That(GetOffset(in s, in s.FilterByProgramId), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.FilterBySaveDataType), Is.EqualTo(1));
        Assert.That(GetOffset(in s, in s.FilterByUserId), Is.EqualTo(2));
        Assert.That(GetOffset(in s, in s.FilterBySaveDataId), Is.EqualTo(3));
        Assert.That(GetOffset(in s, in s.FilterByIndex), Is.EqualTo(4));
        Assert.That(GetOffset(in s, in s.Rank), Is.EqualTo(5));
        Assert.That(GetOffset(in s, in s.Attribute), Is.EqualTo(8));
    }

    [Test]
    public static void SaveDataMetaInfo_Layout()
    {
        var s = new SaveDataMetaInfo();

        Assert.That(Unsafe.SizeOf<SaveDataMetaInfo>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.Type), Is.EqualTo(4));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(5));
    }

    [Test]
    public static void HashSalt_Layout()
    {
        var s = new HashSalt();

        Assert.That(Unsafe.SizeOf<HashSalt>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Hash[0]), Is.EqualTo(0));
    }

    [Test]
    public static void SaveDataInfo_Layout()
    {
        var s = new SaveDataInfo();

        Assert.That(Unsafe.SizeOf<SaveDataInfo>(), Is.EqualTo(0x60));

        Assert.That(GetOffset(in s, in s.SaveDataId), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.SpaceId), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Type), Is.EqualTo(0x09));
        Assert.That(GetOffset(in s, in s.UserId), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.StaticSaveDataId), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.ProgramId), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.Index), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.Rank), Is.EqualTo(0x3A));
        Assert.That(GetOffset(in s, in s.State), Is.EqualTo(0x3B));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x3C));
    }

    [Test]
    public static void SaveDataExtraData_Layout()
    {
        var s = new SaveDataExtraData();

        Assert.That(Unsafe.SizeOf<SaveDataExtraData>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.Attribute), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.OwnerId), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.TimeStamp), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x50));
        Assert.That(GetOffset(in s, in s.FormatType), Is.EqualTo(0x54));
        Assert.That(GetOffset(in s, in s.DataSize), Is.EqualTo(0x58));
        Assert.That(GetOffset(in s, in s.JournalSize), Is.EqualTo(0x60));
        Assert.That(GetOffset(in s, in s.CommitId), Is.EqualTo(0x68));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x70));
    }

    [Test]
    public static void CommitOption_Layout()
    {
        var s = new CommitOption();

        Assert.That(Unsafe.SizeOf<CommitOption>(), Is.EqualTo(4));

        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0));
    }

    [Test]
    public static void MemoryReportInfo_Layout()
    {
        var s = new MemoryReportInfo();

        Assert.That(Unsafe.SizeOf<MemoryReportInfo>(), Is.EqualTo(0x80));

        Assert.That(GetOffset(in s, in s.PooledBufferFreeSizePeak), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.PooledBufferRetriedCount), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.PooledBufferReduceAllocationCount), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.BufferManagerFreeSizePeak), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.BufferManagerRetriedCount), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.ExpHeapFreeSizePeak), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.BufferPoolFreeSizePeak), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.PatrolReadAllocateBufferSuccessCount), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.PatrolReadAllocateBufferFailureCount), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.BufferManagerTotalAllocatableSizePeak), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.BufferPoolAllocateSizeMax), Is.EqualTo(0x50));
        Assert.That(GetOffset(in s, in s.PooledBufferFailedIdealAllocationCountOnAsyncAccess), Is.EqualTo(0x58));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x60));
    }

    [Test]
    public static void ApplicationInfo_Layout()
    {
        var s = new ApplicationInfo();

        Assert.That(Unsafe.SizeOf<ApplicationInfo>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.ApplicationId), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x8));
        Assert.That(GetOffset(in s, in s.LaunchType), Is.EqualTo(0xC));
        Assert.That(GetOffset(in s, in s.IsMultiProgram), Is.EqualTo(0xD));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0xE));
    }

    [Test]
    public static void CodeVerificationData_Layout()
    {
        var s = new CodeVerificationData();

        Assert.That(Unsafe.SizeOf<CodeVerificationData>(), Is.EqualTo(0x124));

        Assert.That(GetOffset(in s, in s.Signature), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Hash), Is.EqualTo(0x100));
        Assert.That(GetOffset(in s, in s.HasData), Is.EqualTo(0x120));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x121));
    }

    [Test]
    public static void DirectoryEntry_Layout()
    {
        var s = new DirectoryEntry();

        Assert.That(Unsafe.SizeOf<DirectoryEntry>(), Is.EqualTo(0x310));

        Assert.That(GetOffset(in s, in s.Name), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Attributes), Is.EqualTo(0x301));
        Assert.That(GetOffset(in s, in s.Reserved302), Is.EqualTo(0x302));
        Assert.That(GetOffset(in s, in s.Type), Is.EqualTo(0x304));
        Assert.That(GetOffset(in s, in s.Reserved305), Is.EqualTo(0x305));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x308));
    }

    [Test]
    public static void EncryptionSeed_Layout()
    {
        var s = new EncryptionSeed();

        Assert.That(Unsafe.SizeOf<EncryptionSeed>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0));
    }

    [Test]
    public static void FileSystemProxyErrorInfo_Layout()
    {
        var s = new FileSystemProxyErrorInfo();

        Assert.That(Unsafe.SizeOf<FileSystemProxyErrorInfo>(), Is.EqualTo(0x80));

        Assert.That(GetOffset(in s, in s.RemountForDataCorruptionCount), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.UnrecoverableDataCorruptionByRemountCount), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.FatFsError), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.RecoveredByInvalidateCacheCount), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.SaveDataIndexCount), Is.EqualTo(0x2C));
        Assert.That(GetOffset(in s, in s.BisSystemFatReportInfo), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.BisUserFatReport), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.SdCardFatReport), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x3C));
    }

    [Test]
    public static void StorageErrorInfo_Layout()
    {
        var s = new StorageErrorInfo();

        Assert.That(Unsafe.SizeOf<StorageErrorInfo>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.NumActivationFailures), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.NumActivationErrorCorrections), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.NumReadWriteFailures), Is.EqualTo(0x8));
        Assert.That(GetOffset(in s, in s.NumReadWriteErrorCorrections), Is.EqualTo(0xC));
    }

    [Test]
    public static void FileSystemAttribute_Layout()
    {
        var s = new FileSystemAttribute();

        Assert.That(Unsafe.SizeOf<FileSystemAttribute>(), Is.EqualTo(0xC0));

        Assert.That(GetOffset(in s, in s.DirectoryNameLengthMaxHasValue), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.FileNameLengthMaxHasValue), Is.EqualTo(0x01));
        Assert.That(GetOffset(in s, in s.DirectoryPathLengthMaxHasValue), Is.EqualTo(0x02));
        Assert.That(GetOffset(in s, in s.FilePathLengthMaxHasValue), Is.EqualTo(0x03));
        Assert.That(GetOffset(in s, in s.Utf16CreateDirectoryPathLengthMaxHasValue), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.Utf16DeleteDirectoryPathLengthMaxHasValue), Is.EqualTo(0x05));
        Assert.That(GetOffset(in s, in s.Utf16RenameSourceDirectoryPathLengthMaxHasValue), Is.EqualTo(0x06));
        Assert.That(GetOffset(in s, in s.Utf16RenameDestinationDirectoryPathLengthMaxHasValue), Is.EqualTo(0x07));
        Assert.That(GetOffset(in s, in s.Utf16OpenDirectoryPathLengthMaxHasValue), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Utf16DirectoryNameLengthMaxHasValue), Is.EqualTo(0x09));
        Assert.That(GetOffset(in s, in s.Utf16FileNameLengthMaxHasValue), Is.EqualTo(0x0A));
        Assert.That(GetOffset(in s, in s.Utf16DirectoryPathLengthMaxHasValue), Is.EqualTo(0x0B));
        Assert.That(GetOffset(in s, in s.Utf16FilePathLengthMaxHasValue), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.Reserved1), Is.EqualTo(0x0D));
        Assert.That(GetOffset(in s, in s.DirectoryNameLengthMax), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.FileNameLengthMax), Is.EqualTo(0x2C));
        Assert.That(GetOffset(in s, in s.DirectoryPathLengthMax), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.FilePathLengthMax), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.Utf16CreateDirectoryPathLengthMax), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.Utf16DeleteDirectoryPathLengthMax), Is.EqualTo(0x3C));
        Assert.That(GetOffset(in s, in s.Utf16RenameSourceDirectoryPathLengthMax), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.Utf16RenameDestinationDirectoryPathLengthMax), Is.EqualTo(0x44));
        Assert.That(GetOffset(in s, in s.Utf16OpenDirectoryPathLengthMax), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.Utf16DirectoryNameLengthMax), Is.EqualTo(0x4C));
        Assert.That(GetOffset(in s, in s.Utf16FileNameLengthMax), Is.EqualTo(0x50));
        Assert.That(GetOffset(in s, in s.Utf16DirectoryPathLengthMax), Is.EqualTo(0x54));
        Assert.That(GetOffset(in s, in s.Utf16FilePathLengthMax), Is.EqualTo(0x58));
        Assert.That(GetOffset(in s, in s.Reserved2), Is.EqualTo(0x5C));
    }

    [Test]
    public static void FileTimeStamp_Layout()
    {
        var s = new FileTimeStamp();

        Assert.That(Unsafe.SizeOf<FileTimeStamp>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Created), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Accessed), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Modified), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.IsLocalTime), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x19));
    }

    [Test]
    public static void FileTimeStampRaw_Layout()
    {
        var s = new FileTimeStampRaw();

        Assert.That(Unsafe.SizeOf<FileTimeStampRaw>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Created), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Accessed), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Modified), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.IsLocalTime), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x19));
    }

    [Test]
    public static void ProgramIndexMapInfo_Layout()
    {
        var s = new ProgramIndexMapInfo();

        Assert.That(Unsafe.SizeOf<ProgramIndexMapInfo>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.ProgramId), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.MainProgramId), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.ProgramIndex), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x11));
    }

    [Test]
    public static void QueryRangeInfo_Layout()
    {
        var s = new QueryRangeInfo();

        Assert.That(Unsafe.SizeOf<QueryRangeInfo>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.AesCtrKeyType), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.SpeedEmulationType), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x08));
    }

    [Test]
    public static void RightsId_Layout()
    {
        var s = new RightsId();

        Assert.That(Unsafe.SizeOf<RightsId>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x00));
    }

    [Test]
    public static void RsaEncryptedKey_Layout()
    {
        var s = new RsaEncryptedKey();

        Assert.That(Unsafe.SizeOf<RsaEncryptedKey>(), Is.EqualTo(0x100));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x00));
    }

    [Test]
    public static void AesKey_Layout()
    {
        var s = new AesKey();

        Assert.That(Unsafe.SizeOf<AesKey>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x00));
    }

    [Test]
    public static void InitialDataAad_Layout()
    {
        var s = new InitialDataAad();

        Assert.That(Unsafe.SizeOf<InitialDataAad>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x00));
    }

    [Test]
    public static void KeySeed_Layout()
    {
        var s = new KeySeed();

        Assert.That(Unsafe.SizeOf<KeySeed>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x00));
    }

    [Test]
    public static void InitialDataMac_Layout()
    {
        var s = new InitialDataMac();

        Assert.That(Unsafe.SizeOf<InitialDataMac>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x00));
    }

    [Test]
    public static void ExportReportInfo_Layout()
    {
        var s = new ExportReportInfo();

        Assert.That(Unsafe.SizeOf<ExportReportInfo>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.DiffChunkCount), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.DoubleDivisionDiffChunkCount), Is.EqualTo(1));
        Assert.That(GetOffset(in s, in s.HalfDivisionDiffChunkCount), Is.EqualTo(2));
        Assert.That(GetOffset(in s, in s.CompressionRate), Is.EqualTo(3));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(4));
    }

    [Test]
    public static void ImportReportInfo_Layout()
    {
        var s = new ImportReportInfo();

        Assert.That(Unsafe.SizeOf<ImportReportInfo>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.DiffChunkCount), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.DoubleDivisionDiffChunkCount), Is.EqualTo(1));
        Assert.That(GetOffset(in s, in s.HalfDivisionDiffChunkCount), Is.EqualTo(2));
        Assert.That(GetOffset(in s, in s.CompressionRate), Is.EqualTo(3));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(4));
    }

    [Test]
    public static void CacheStorageInfo_Layout()
    {
        var s = new CacheStorageInfo();

        Assert.That(Unsafe.SizeOf<CacheStorageInfo>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Index), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(4));
    }

    [Test]
    public static void Challenge_Layout()
    {
        var s = new SaveDataTransferManagerVersion2.Challenge();

        Assert.That(Unsafe.SizeOf<SaveDataTransferManagerVersion2.Challenge>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0));
    }

    [Test]
    public static void SaveDataTag_Layout()
    {
        var s = new SaveDataTransferManagerVersion2.SaveDataTag();

        Assert.That(Unsafe.SizeOf<SaveDataTransferManagerVersion2.SaveDataTag>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0));
    }

    [Test]
    public static void KeySeedPackage_Layout()
    {
        var s = new SaveDataTransferManagerVersion2.KeySeedPackage();

        Assert.That(Unsafe.SizeOf<SaveDataTransferManagerVersion2.KeySeedPackage>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0));
    }

    [Test]
    public static void InitialDataVersion2_Layout()
    {
        var s = new InitialDataVersion2();

        Assert.That(Unsafe.SizeOf<InitialDataVersion2>(), Is.EqualTo(0x2000));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0));
    }

    [Test]
    public static void GameCardErrorInfo_Layout()
    {
        var s = new GameCardErrorInfo();

        Assert.That(Unsafe.SizeOf<GameCardErrorInfo>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.GameCardCrcErrorCount), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Reserved2), Is.EqualTo(0x2));
        Assert.That(GetOffset(in s, in s.AsicCrcErrorCount), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.Reserved6), Is.EqualTo(0x6));
        Assert.That(GetOffset(in s, in s.RefreshCount), Is.EqualTo(0x8));
        Assert.That(GetOffset(in s, in s.ReservedA), Is.EqualTo(0xA));
        Assert.That(GetOffset(in s, in s.ReadRetryCount), Is.EqualTo(0xC));
        Assert.That(GetOffset(in s, in s.TimeoutRetryErrorCount), Is.EqualTo(0xE));
    }

    [Test]
    public static void GameCardErrorReportInfo_Layout()
    {
        var s = new GameCardErrorReportInfo();

        Assert.That(Unsafe.SizeOf<GameCardErrorReportInfo>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.ErrorInfo), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.AsicReinitializeFailureDetail), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.InsertionCount), Is.EqualTo(0x12));
        Assert.That(GetOffset(in s, in s.RemovalCount), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.AsicReinitializeCount), Is.EqualTo(0x16));
        Assert.That(GetOffset(in s, in s.AsicInitializeCount), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.AsicReinitializeFailureCount), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.AwakenFailureCount), Is.EqualTo(0x1E));
        Assert.That(GetOffset(in s, in s.Reserved20), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.RefreshCount), Is.EqualTo(0x22));
        Assert.That(GetOffset(in s, in s.LastReadErrorPageAddress), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.LastReadErrorPageCount), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.AwakenCount), Is.EqualTo(0x2C));
        Assert.That(GetOffset(in s, in s.ReadCountFromInsert), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.ReadCountFromAwaken), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.Reserved38), Is.EqualTo(0x38));
    }

    [Test]
    public static void GameCardUpdatePartitionInfo_Layout()
    {
        var s = new GameCardUpdatePartitionInfo();

        Assert.That(Unsafe.SizeOf<GameCardUpdatePartitionInfo>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.CupVersion), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.CupId), Is.EqualTo(8));
    }

    [Test]
    public static void Int64_Layout()
    {
        Assert.That(Unsafe.SizeOf<LibHac.Fs.Int64>(), Is.EqualTo(8));
        Assert.That(AlignOf<LibHac.Fs.Int64>(), Is.EqualTo(4));
    }
}