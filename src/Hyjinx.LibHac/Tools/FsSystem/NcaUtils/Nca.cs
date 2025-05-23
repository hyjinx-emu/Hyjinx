using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Diag;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem.RomFs;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

public partial class Nca
{
    private KeySet KeySet { get; }
    public IStorage BaseStorage { get; }
    public NcaHeader Header { get; }

    public Nca(KeySet keySet, IStorage storage)
    {
        KeySet = keySet;
        BaseStorage = storage;
        Header = new NcaHeader(storage);
    }

    public bool CanOpenSection(NcaSectionType type)
    {
        if (!TryGetSectionIndexFromType(type, Header.ContentType, out int index))
        {
            return false;
        }

        return CanOpenSection(index);
    }

    public bool CanOpenSection(int index)
    {
        if (!SectionExists(index))
            return false;
        if (GetFsHeader(index).EncryptionType == NcaEncryptionType.None)
            return true;

        return false;
    }

    public bool SectionExists(NcaSectionType type)
    {
        if (!TryGetSectionIndexFromType(type, Header.ContentType, out int index))
        {
            return false;
        }

        return SectionExists(index);
    }

    internal bool SectionExists(int index)
    {
        return Header.IsSectionEnabled(index);
    }

    internal NcaFsHeader GetFsHeader(int index)
    {
        if (Header.IsNca0())
            return GetNca0FsHeader(index);

        return Header.GetFsHeader(index);
    }

    private IStorage OpenSectionStorage(int index)
    {
        if (!SectionExists(index))
            throw new ArgumentException(string.Format(Messages.NcaSectionMissing, index), nameof(index));

        long offset = Header.GetSectionStartOffset(index);
        long size = Header.GetSectionSize(index);

        BaseStorage.GetSize(out long ncaStorageSize).ThrowIfFailure();

        if (!IsSubRange(offset, size, ncaStorageSize))
        {
            throw new InvalidDataException(
                $"Section offset (0x{offset:x}) and length (0x{size:x}) fall outside the total NCA length (0x{ncaStorageSize:x}).");
        }

        return BaseStorage.Slice(offset, size);
    }

    internal IStorage OpenRawStorage(int index)
    {
        if (Header.IsNca0())
            return OpenNca0RawStorage(index);

        return OpenSectionStorage(index);
    }

    private IStorage OpenRawStorageWithPatch(Nca patchNca, int index)
    {
        IStorage patchStorage = patchNca.OpenRawStorage(index);
        IStorage baseStorage = SectionExists(index) ? OpenRawStorage(index) : new NullStorage();

        patchStorage.GetSize(out long patchSize).ThrowIfFailure();
        baseStorage.GetSize(out long baseSize).ThrowIfFailure();

        NcaFsHeader header = patchNca.GetFsHeader(index);
        NcaFsPatchInfo patchInfo = header.GetPatchInfo();

        if (patchInfo.RelocationTreeSize == 0)
        {
            return patchStorage;
        }

        var treeHeader = new BucketTree.Header();
        patchInfo.RelocationTreeHeader.CopyTo(SpanHelpers.AsByteSpan(ref treeHeader));
        long nodeStorageSize = IndirectStorage.QueryNodeStorageSize(treeHeader.EntryCount);
        long entryStorageSize = IndirectStorage.QueryEntryStorageSize(treeHeader.EntryCount);

        var relocationTableStorage = new SubStorage(patchStorage, patchInfo.RelocationTreeOffset, patchInfo.RelocationTreeSize);
        var cachedTableStorage = new CachedStorage(relocationTableStorage, IndirectStorage.NodeSize, 4, true);

        using var tableNodeStorage = new ValueSubStorage(cachedTableStorage, 0, nodeStorageSize);
        using var tableEntryStorage = new ValueSubStorage(cachedTableStorage, nodeStorageSize, entryStorageSize);

        var storage = new IndirectStorage();
        storage.Initialize(new ArrayPoolMemoryResource(), in tableNodeStorage, in tableEntryStorage, treeHeader.EntryCount).ThrowIfFailure();

        storage.SetStorage(0, baseStorage, 0, baseSize);
        storage.SetStorage(1, patchStorage, 0, patchSize);

        return storage;
    }

    public IStorage OpenStorage(int index, IntegrityCheckLevel integrityCheckLevel, bool leaveCompressed = false)
    {
        IStorage rawStorage = OpenRawStorage(index);
        NcaFsHeader header = GetFsHeader(index);

        IStorage returnStorage = CreateVerificationStorage(integrityCheckLevel, header, rawStorage);

        if (!leaveCompressed && header.ExistsCompressionLayer())
        {
            returnStorage = OpenCompressedStorage(header, returnStorage);
        }

        return returnStorage;
    }

    public IStorage OpenStorageWithPatch(Nca patchNca, int index, IntegrityCheckLevel integrityCheckLevel, bool leaveCompressed = false)
    {
        IStorage rawStorage = OpenRawStorageWithPatch(patchNca, index);
        NcaFsHeader header = patchNca.GetFsHeader(index);

        IStorage returnStorage = CreateVerificationStorage(integrityCheckLevel, header, rawStorage);

        if (!leaveCompressed && header.ExistsCompressionLayer())
        {
            returnStorage = OpenCompressedStorage(header, returnStorage);
        }

        return returnStorage;
    }

    private static IStorage OpenCompressedStorage(NcaFsHeader header, IStorage baseStorage)
    {
        ref NcaCompressionInfo compressionInfo = ref header.GetCompressionInfo();

        Unsafe.SkipInit(out BucketTree.Header bucketTreeHeader);
        compressionInfo.TableHeader.ItemsRo.CopyTo(SpanHelpers.AsByteSpan(ref bucketTreeHeader));
        bucketTreeHeader.Verify().ThrowIfFailure();

        long nodeStorageSize = CompressedStorage.QueryNodeStorageSize(bucketTreeHeader.EntryCount);
        long entryStorageSize = CompressedStorage.QueryEntryStorageSize(bucketTreeHeader.EntryCount);
        long tableOffset = compressionInfo.TableOffset;
        long tableSize = compressionInfo.TableSize;

        if (entryStorageSize + nodeStorageSize > tableSize)
            throw new HorizonResultException(ResultFs.NcaInvalidCompressionInfo.Value);

        using var dataStorage = new ValueSubStorage(baseStorage, 0, tableOffset);
        using var nodeStorage = new ValueSubStorage(baseStorage, tableOffset, nodeStorageSize);
        using var entryStorage = new ValueSubStorage(baseStorage, tableOffset + nodeStorageSize, entryStorageSize);

        var compressedStorage = new CompressedStorage();
        compressedStorage.Initialize(new ArrayPoolMemoryResource(), in dataStorage, in nodeStorage, in entryStorage,
            bucketTreeHeader.EntryCount).ThrowIfFailure();

        return new CachedStorage(compressedStorage, 0x4000, 32, true);
    }

    private IStorage CreateVerificationStorage(IntegrityCheckLevel integrityCheckLevel, NcaFsHeader header, IStorage rawStorage)
    {
        switch (header.HashType)
        {
            case NcaHashType.Sha256:
                return InitIvfcForPartitionFs(header.GetIntegrityInfoSha256(), rawStorage, integrityCheckLevel, true);
            case NcaHashType.Ivfc:
                // The FS header of an NCA0 section with IVFC verification must be manually skipped
                if (Header.IsNca0())
                {
                    rawStorage = rawStorage.Slice(0x200);
                }

                return InitIvfcForRomFs(header.GetIntegrityInfoIvfc(), rawStorage, integrityCheckLevel, true);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IFileSystem OpenFileSystem(int index, IntegrityCheckLevel integrityCheckLevel)
    {
        IStorage storage = OpenStorage(index, integrityCheckLevel);
        NcaFsHeader header = GetFsHeader(index);

        return OpenFileSystem(storage, header);
    }

    public IFileSystem OpenFileSystemWithPatch(Nca patchNca, int index, IntegrityCheckLevel integrityCheckLevel)
    {
        IStorage storage = OpenStorageWithPatch(patchNca, index, integrityCheckLevel);
        NcaFsHeader header = patchNca.GetFsHeader(index);

        return OpenFileSystem(storage, header);
    }

    private IFileSystem OpenFileSystem(IStorage storage, NcaFsHeader header)
    {
        switch (header.FormatType)
        {
            case NcaFormatType.Pfs0:
                var pfs = new PartitionFileSystem();
                pfs.Initialize(storage).ThrowIfFailure();
                return pfs;
            case NcaFormatType.Romfs:
                return new RomFsFileSystem(storage);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public IFileSystem OpenFileSystem(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        return OpenFileSystem(GetSectionIndexFromType(type), integrityCheckLevel);
    }

    public IFileSystem OpenFileSystemWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        return OpenFileSystemWithPatch(patchNca, GetSectionIndexFromType(type), integrityCheckLevel);
    }

    public IStorage OpenStorage(NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        return OpenStorage(GetSectionIndexFromType(type), integrityCheckLevel);
    }

    public IStorage OpenStorageWithPatch(Nca patchNca, NcaSectionType type, IntegrityCheckLevel integrityCheckLevel)
    {
        return OpenStorageWithPatch(patchNca, GetSectionIndexFromType(type), integrityCheckLevel);
    }

    private int GetSectionIndexFromType(NcaSectionType type)
    {
        return GetSectionIndexFromType(type, Header.ContentType);
    }

    public static int GetSectionIndexFromType(NcaSectionType type, NcaContentType contentType)
    {
        if (!TryGetSectionIndexFromType(type, contentType, out int index))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "NCA does not contain this section type.");
        }

        return index;
    }

    private static bool TryGetSectionIndexFromType(NcaSectionType type, NcaContentType contentType, out int index)
    {
        switch (type)
        {
            case NcaSectionType.Code when contentType == NcaContentType.Program:
                index = 0;
                return true;
            case NcaSectionType.Data when contentType == NcaContentType.Program:
                index = 1;
                return true;
            case NcaSectionType.Logo when contentType == NcaContentType.Program:
                index = 2;
                return true;
            case NcaSectionType.Data:
                index = 0;
                return true;
            default:
                index = 0;
                return false;
        }
    }

    private static HierarchicalIntegrityVerificationStorage InitIvfcForPartitionFs(NcaFsIntegrityInfoSha256 info,
        IStorage pfsStorage, IntegrityCheckLevel integrityCheckLevel, bool leaveOpen)
    {
        Debug.Assert(info.LevelCount == 2);

        IStorage hashStorage = pfsStorage.Slice(info.GetLevelOffset(0), info.GetLevelSize(0), leaveOpen);
        IStorage dataStorage = pfsStorage.Slice(info.GetLevelOffset(1), info.GetLevelSize(1), leaveOpen);

        var initInfo = new IntegrityVerificationInfo[3];

        // Set the master hash
        initInfo[0] = new IntegrityVerificationInfo
        {
            // todo Get hash directly from header
            Data = new MemoryStorage(info.MasterHash.ToArray()),

            BlockSize = 0,
            Type = IntegrityStorageType.PartitionFs
        };

        initInfo[1] = new IntegrityVerificationInfo
        {
            Data = hashStorage,
            BlockSize = (int)info.GetLevelSize(0),
            Type = IntegrityStorageType.PartitionFs
        };

        initInfo[2] = new IntegrityVerificationInfo
        {
            Data = dataStorage,
            BlockSize = info.BlockSize,
            Type = IntegrityStorageType.PartitionFs
        };

        return new HierarchicalIntegrityVerificationStorage(initInfo, integrityCheckLevel, leaveOpen);
    }

    private static HierarchicalIntegrityVerificationStorage InitIvfcForRomFs(NcaFsIntegrityInfoIvfc ivfc,
        IStorage dataStorage, IntegrityCheckLevel integrityCheckLevel, bool leaveOpen)
    {
        var initInfo = new IntegrityVerificationInfo[ivfc.LevelCount];

        initInfo[0] = new IntegrityVerificationInfo
        {
            Data = new MemoryStorage(ivfc.MasterHash.ToArray()),
            BlockSize = 0
        };

        for (int i = 1; i < ivfc.LevelCount; i++)
        {
            initInfo[i] = new IntegrityVerificationInfo
            {
                Data = dataStorage.Slice(ivfc.GetLevelOffset(i - 1), ivfc.GetLevelSize(i - 1)),
                BlockSize = 1 << ivfc.GetLevelBlockSize(i - 1),
                Type = IntegrityStorageType.RomFs
            };
        }

        return new HierarchicalIntegrityVerificationStorage(initInfo, integrityCheckLevel, leaveOpen);
    }

    private IStorage OpenNca0BodyStorage()
    {
        Assert.SdkEqual(0, Header.Version);

        BaseStorage.GetSize(out long ncaSize).ThrowIfFailure();
        return BaseStorage.Slice(0x400, ncaSize - 0x400);
    }

    private IStorage OpenNca0RawStorage(int index)
    {
        if (!SectionExists(index))
            throw new ArgumentException(string.Format(Messages.NcaSectionMissing, index), nameof(index));

        long offset = Header.GetSectionStartOffset(index) - 0x400;
        long size = Header.GetSectionSize(index);

        IStorage bodyStorage = OpenNca0BodyStorage();

        bodyStorage.GetSize(out long baseSize).ThrowIfFailure();

        if (!IsSubRange(offset, size, baseSize))
        {
            throw new InvalidDataException(
                $"Section offset (0x{offset + 0x400:x}) and length (0x{size:x}) fall outside the total NCA length (0x{baseSize + 0x400:x}).");
        }

        return new SubStorage(bodyStorage, offset, size);
    }

    private NcaFsHeader GetNca0FsHeader(int index)
    {
        // NCA0 stores the FS header in the first block of the section instead of the header
        IStorage bodyStorage = OpenNca0BodyStorage();
        long offset = Header.GetSectionStartOffset(index) - 0x400;

        byte[] fsHeaderData = new byte[0x200];
        bodyStorage.Read(offset, fsHeaderData).ThrowIfFailure();

        return new NcaFsHeader(fsHeaderData);
    }

    private static bool IsSubRange(long startIndex, long subLength, long length)
    {
        bool isOutOfRange = startIndex < 0 || startIndex > length || subLength < 0 || startIndex > length - subLength;
        return !isOutOfRange;
    }
}