using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tests.Fs;
using LibHac.Tests.Fs.IFileSystemTestBase;
using LibHac.Tools.Fs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Tests.FsSystem;

public class DirectorySaveDataFileSystemTests : CommittableIFileSystemTests
{
    private static void AssertSuccess(Result result)
    {
        Assert.That(result.IsSuccess(), Is.True, result.ToString());
    }

    private static void AssertResult(Result.Base expected, Result actual)
    {
        Assert.That(expected.Includes(actual), Is.True, actual.ToString());
    }

    protected override IFileSystem CreateFileSystem()
    {
        return CreateFileSystemInternal().saveFs;
    }

    protected override IReopenableFileSystemCreator GetFileSystemCreator()
    {
        return new DirectorySaveDataFileSystemCreator();
    }

    private class DirectorySaveDataFileSystemCreator : IReopenableFileSystemCreator
    {
        private IFileSystem BaseFileSystem { get; }

        public DirectorySaveDataFileSystemCreator()
        {
            BaseFileSystem = new InMemoryFileSystem();
        }

        public IFileSystem Create()
        {
            CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, BaseFileSystem, true, true, true)
                .ThrowIfFailure();

            return saveFs;
        }
    }

    public static Result CreateDirSaveFs(out DirectorySaveDataFileSystem created, IFileSystem baseFileSystem,
        ISaveDataCommitTimeStampGetter timeStampGetter, RandomDataGenerator randomGenerator,
        bool isJournalingSupported, bool isMultiCommitSupported, bool isJournalingEnabled,
        FileSystemClient fsClient)
    {
        var obj = new DirectorySaveDataFileSystem(baseFileSystem, fsClient);
        Result res = obj.Initialize(isJournalingSupported, isMultiCommitSupported, isJournalingEnabled, timeStampGetter,
            randomGenerator);

        if (res.IsSuccess())
        {
            created = obj;
            return Result.Success;
        }

        obj.Dispose();
        UnsafeHelpers.SkipParamInit(out created);
        return res;
    }

    public static Result CreateDirSaveFs(out DirectorySaveDataFileSystem created, IFileSystem baseFileSystem,
        bool isJournalingSupported, bool isMultiCommitSupported, bool isJournalingEnabled)
    {
        return CreateDirSaveFs(out created, baseFileSystem, null, null, isJournalingSupported, isMultiCommitSupported,
            isJournalingEnabled, null);
    }

    private (IFileSystem baseFs, DirectorySaveDataFileSystem saveFs) CreateFileSystemInternal()
    {
        var baseFs = new InMemoryFileSystem();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        return (baseFs, saveFs);
    }

    [Test]
    public void CreateFile_CreatedInModifiedDirectory()
    {
        (IFileSystem baseFs, IFileSystem saveFs) = CreateFileSystemInternal();

        saveFs.CreateFile("/file", 0, CreateFileOptions.None);

        AssertSuccess(baseFs.GetEntryType(out DirectoryEntryType type, "/1/file"));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void CreateFile_NotCreatedInCommittedDirectory()
    {
        (IFileSystem baseFs, IFileSystem saveFs) = CreateFileSystemInternal();

        saveFs.CreateFile("/file", 0, CreateFileOptions.None);

        AssertResult(ResultFs.PathNotFound, baseFs.GetEntryType(out _, "/0/file"));
    }

    [Test]
    public void Commit_FileExistsInCommittedDirectory()
    {
        (IFileSystem baseFs, IFileSystem saveFs) = CreateFileSystemInternal();

        saveFs.CreateFile("/file", 0, CreateFileOptions.None);

        AssertSuccess(saveFs.Commit());

        AssertSuccess(baseFs.GetEntryType(out DirectoryEntryType type, "/0/file"));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void Rollback_FileDoesNotExistInBaseAfterRollback()
    {
        (IFileSystem baseFs, IFileSystem saveFs) = CreateFileSystemInternal();

        saveFs.CreateFile("/file", 0, CreateFileOptions.None);

        // Rollback should succeed
        AssertSuccess(saveFs.Rollback());

        // Make sure all the files are gone
        AssertResult(ResultFs.PathNotFound, saveFs.GetEntryType(out _, "/file"));
        AssertResult(ResultFs.PathNotFound, baseFs.GetEntryType(out _, "/0/file"));
        AssertResult(ResultFs.PathNotFound, baseFs.GetEntryType(out _, "/1/file"));
    }

    [Test]
    public void Rollback_DeletedFileIsRestoredInBaseAfterRollback()
    {
        (IFileSystem baseFs, IFileSystem saveFs) = CreateFileSystemInternal();

        saveFs.CreateFile("/file", 0, CreateFileOptions.None);
        saveFs.Commit();
        saveFs.DeleteFile("/file");

        // Rollback should succeed
        AssertSuccess(saveFs.Rollback());

        // Make sure all the files are restored
        AssertSuccess(saveFs.GetEntryType(out _, "/file"));
        AssertSuccess(baseFs.GetEntryType(out _, "/0/file"));
        AssertSuccess(baseFs.GetEntryType(out _, "/1/file"));
    }

    [Test]
    public void Initialize_NormalState_UsesCommittedData()
    {
        var baseFs = new InMemoryFileSystem();

        baseFs.CreateDirectory("/0").ThrowIfFailure();
        baseFs.CreateDirectory("/1").ThrowIfFailure();

        // Set the existing files before initializing the save FS
        baseFs.CreateFile("/0/file1", 0, CreateFileOptions.None).ThrowIfFailure();
        baseFs.CreateFile("/1/file2", 0, CreateFileOptions.None).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        AssertSuccess(saveFs.GetEntryType(out _, "/file1"));
        AssertResult(ResultFs.PathNotFound, saveFs.GetEntryType(out _, "/file2"));
    }

    [Test]
    public void Initialize_InterruptedAfterCommitPart1_UsesModifiedData()
    {
        var baseFs = new InMemoryFileSystem();

        baseFs.CreateDirectory("/_").ThrowIfFailure();
        baseFs.CreateDirectory("/1").ThrowIfFailure();

        // Set the existing files before initializing the save FS
        baseFs.CreateFile("/_/file1", 0, CreateFileOptions.None).ThrowIfFailure();
        baseFs.CreateFile("/1/file2", 0, CreateFileOptions.None).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        AssertResult(ResultFs.PathNotFound, saveFs.GetEntryType(out _, "/file1"));
        AssertSuccess(saveFs.GetEntryType(out _, "/file2"));
    }

    [Test]
    public void Initialize_InterruptedDuringCommitPart2_UsesModifiedData()
    {
        var baseFs = new InMemoryFileSystem();

        baseFs.CreateDirectory("/1").ThrowIfFailure();

        // Set the existing files before initializing the save FS
        baseFs.CreateFile("/1/file2", 0, CreateFileOptions.None).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        AssertResult(ResultFs.PathNotFound, saveFs.GetEntryType(out _, "/file1"));
        AssertSuccess(saveFs.GetEntryType(out _, "/file2"));
    }

    [Test]
    public void Initialize_InitialExtraDataIsEmpty()
    {
        (IFileSystem _, DirectorySaveDataFileSystem saveFs) = CreateFileSystemInternal();

        AssertSuccess(saveFs.ReadExtraData(out SaveDataExtraData extraData));
        Assert.That(SpanHelpers.AsByteSpan(ref extraData).IsZeros(), Is.True);
    }

    [Test]
    public void WriteExtraData_CanReadBackExtraData()
    {
        (IFileSystem _, DirectorySaveDataFileSystem saveFs) = CreateFileSystemInternal();

        var originalExtraData = new SaveDataExtraData();
        originalExtraData.DataSize = 0x12345;

        AssertSuccess(saveFs.WriteExtraData(in originalExtraData));
        AssertSuccess(saveFs.ReadExtraData(out SaveDataExtraData extraData));
        Assert.That(extraData, Is.EqualTo(originalExtraData));
    }

    [Test]
    public void Commit_AfterSuccessfulCommit_CanReadCommittedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        var originalExtraData = new SaveDataExtraData();
        originalExtraData.DataSize = 0x12345;

        saveFs.WriteExtraData(in originalExtraData).ThrowIfFailure();
        AssertSuccess(saveFs.CommitExtraData(false));

        saveFs.Dispose();
        CreateDirSaveFs(out saveFs, baseFs, true, true, true).ThrowIfFailure();

        AssertSuccess(saveFs.ReadExtraData(out SaveDataExtraData extraData));
        Assert.That(extraData, Is.EqualTo(originalExtraData));
    }

    [Test]
    public void Rollback_WriteExtraDataThenRollback_ExtraDataIsRolledBack()
    {
        var baseFs = new InMemoryFileSystem();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        var originalExtraData = new SaveDataExtraData();
        originalExtraData.DataSize = 0x12345;

        saveFs.WriteExtraData(in originalExtraData).ThrowIfFailure();
        saveFs.CommitExtraData(false).ThrowIfFailure();

        saveFs.Dispose();
        CreateDirSaveFs(out saveFs, baseFs, true, true, true).ThrowIfFailure();

        var newExtraData = new SaveDataExtraData();
        newExtraData.DataSize = 0x67890;

        saveFs.WriteExtraData(in newExtraData).ThrowIfFailure();

        AssertSuccess(saveFs.Rollback());
        AssertSuccess(saveFs.ReadExtraData(out SaveDataExtraData extraData));

        Assert.That(extraData, Is.EqualTo(originalExtraData));
    }

    [Test]
    public void Rollback_WriteExtraDataThenCloseFs_ExtraDataIsRolledBack()
    {
        var baseFs = new InMemoryFileSystem();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        // Write extra data and close with committing
        var originalExtraData = new SaveDataExtraData();
        originalExtraData.DataSize = 0x12345;

        saveFs.WriteExtraData(in originalExtraData).ThrowIfFailure();
        saveFs.CommitExtraData(false).ThrowIfFailure();

        saveFs.Dispose();
        CreateDirSaveFs(out saveFs, baseFs, true, true, true).ThrowIfFailure();

        // Write a new extra data and close without committing
        var newExtraData = new SaveDataExtraData();
        newExtraData.DataSize = 0x67890;

        saveFs.WriteExtraData(in newExtraData).ThrowIfFailure();
        saveFs.Dispose();

        // Read extra data should match the first one
        CreateDirSaveFs(out saveFs, baseFs, true, true, true).ThrowIfFailure();
        AssertSuccess(saveFs.ReadExtraData(out SaveDataExtraData extraData));

        Assert.That(extraData, Is.EqualTo(originalExtraData));
    }

    [Test]
    public void Initialize_InterruptedAfterCommitPart1_UsesModifiedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData_", 0x12345).ThrowIfFailure();
        CreateExtraDataForTest(baseFs, "/ExtraData1", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x67890));
    }

    [Test]
    public void Initialize_OnlySynchronizingExtraDataExists_UsesSynchronizingExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData_", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x67890));
    }

    [Test]
    public void Initialize_OnlyCommittedExtraDataExists_UsesCommittedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData0", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x67890));
    }

    [Test]
    public void Initialize_OnlyModifiedExtraDataExists_UsesModifiedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData1", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x67890));
    }

    [Test]
    public void Initialize_SynchronizingAndCommittedExtraDataExist_UsesCommittedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData_", 0x12345).ThrowIfFailure();
        CreateExtraDataForTest(baseFs, "/ExtraData0", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x67890));
    }

    [Test]
    public void Initialize_SynchronizingAndModifiedExtraDataExist_UsesModifiedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData_", 0x12345).ThrowIfFailure();
        CreateExtraDataForTest(baseFs, "/ExtraData1", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x67890));
    }

    [Test]
    public void Initialize_CommittedAndModifiedExtraDataExist_UsesCommittedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData0", 0x12345).ThrowIfFailure();
        CreateExtraDataForTest(baseFs, "/ExtraData1", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x12345));
    }

    [Test]
    public void Initialize_AllThreeExtraDataFilesExist_UsesCommittedExtraData()
    {
        var baseFs = new InMemoryFileSystem();

        CreateExtraDataForTest(baseFs, "/ExtraData_", 0x12345).ThrowIfFailure();
        CreateExtraDataForTest(baseFs, "/ExtraData0", 0x54321).ThrowIfFailure();
        CreateExtraDataForTest(baseFs, "/ExtraData1", 0x67890).ThrowIfFailure();

        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, true, true, true).ThrowIfFailure();

        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();

        Assert.That(extraData.DataSize, Is.EqualTo(0x54321));
    }

    [Test]
    public void CommitSaveData_MultipleCommits_CommitIdIsUpdatedSkippingInvalidIds()
    {
        var random = new RandomGenerator();
        RandomDataGenerator randomGeneratorFunc = buffer => random.GenerateRandom(buffer);
        var timeStampGetter = new TimeStampGetter();

        var baseFs = new InMemoryFileSystem();
        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, timeStampGetter,
            randomGeneratorFunc, true, true, true, null).ThrowIfFailure();

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();
        Assert.That(extraData.CommitId, Is.EqualTo(2));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.CommitId, Is.EqualTo(3));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.CommitId, Is.EqualTo(6));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.CommitId, Is.EqualTo(2));
    }

    [Test]
    public void CommitSaveData_MultipleCommits_TimeStampUpdated()
    {
        var random = new RandomGenerator();
        RandomDataGenerator randomGeneratorFunc = buffer => random.GenerateRandom(buffer);
        var timeStampGetter = new TimeStampGetter();

        var baseFs = new InMemoryFileSystem();
        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, timeStampGetter,
            randomGeneratorFunc, true, true, true, null).ThrowIfFailure();

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(1u));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(2u));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(3u));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(4u));
    }

    [Test]
    public void CommitSaveData_UpdateTimeStampIsFalse_TimeStampAndCommitIdAreNotUpdated()
    {
        var random = new RandomGenerator();
        RandomDataGenerator randomGeneratorFunc = buffer => random.GenerateRandom(buffer);
        var timeStampGetter = new TimeStampGetter();

        var baseFs = new InMemoryFileSystem();
        CreateDirSaveFs(out DirectorySaveDataFileSystem saveFs, baseFs, timeStampGetter,
            randomGeneratorFunc, true, true, true, null).ThrowIfFailure();

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out SaveDataExtraData extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(1u));
        Assert.That(extraData.CommitId, Is.EqualTo(2));

        saveFs.CommitExtraData(false).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(1u));
        Assert.That(extraData.CommitId, Is.EqualTo(2));

        saveFs.CommitExtraData(true).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(2u));
        Assert.That(extraData.CommitId, Is.EqualTo(3));

        saveFs.CommitExtraData(false).ThrowIfFailure();
        saveFs.ReadExtraData(out extraData).ThrowIfFailure();
        Assert.That(extraData.TimeStamp, Is.EqualTo(2u));
        Assert.That(extraData.CommitId, Is.EqualTo(3));
    }

    private class TimeStampGetter : ISaveDataCommitTimeStampGetter
    {
        private long _currentTimeStamp = 1;

        public Result Get(out long timeStamp)
        {
            timeStamp = _currentTimeStamp++;
            return Result.Success;
        }
    }

    private class RandomGenerator
    {
        private static readonly int[] Values = { 2, 0, 3, 3, 6, 0 };

        private int _index;

        public void GenerateRandom(Span<byte> output)
        {
            if (output.Length != 8)
                throw new ArgumentException();

            Unsafe.As<byte, long>(ref MemoryMarshal.GetReference(output)) = Values[_index];

            _index = (_index + 1) % Values.Length;
        }
    }

    private Result CreateExtraDataForTest(IFileSystem fileSystem, string path, int saveDataSize)
    {
        fileSystem.DeleteFile(path).IgnoreResult();

        Result res = fileSystem.CreateFile(path, Unsafe.SizeOf<SaveDataExtraData>());
        if (res.IsFailure())
            return res.Miss();

        var extraData = new SaveDataExtraData();
        extraData.DataSize = saveDataSize;

        using var file = new UniqueRef<IFile>();
        res = fileSystem.OpenFile(ref file.Ref, path, OpenMode.ReadWrite);
        if (res.IsFailure())
            return res.Miss();

        res = file.Get.Write(0, SpanHelpers.AsByteSpan(ref extraData), WriteOption.Flush);
        if (res.IsFailure())
            return res.Miss();

        return Result.Success;
    }
}