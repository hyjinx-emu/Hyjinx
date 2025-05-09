#if IS_TPM_BYPASS_ENABLED
#pragma warning disable CS0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Diag;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Spl;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using KeyType = LibHac.Common.Keys.KeyType;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca
{
    [Obsolete("This property can no longer be used due to TPM restrictions.")]
    private bool IsEncrypted => Header.IsEncrypted;
    
    private byte[]? Nca0KeyArea { get; set; }
    private IStorage? Nca0TransformedBody { get; set; }
    
    /// <summary>
    /// Defines the names of the key area keys.
    /// </summary>
    private static readonly string[] KakNames = [ "application", "ocean", "system" ];
    
    public Nca(KeySet keySet, IStorage storage)
    {
        KeySet = keySet;
        BaseStorage = storage;
        Header = new NcaHeader(keySet, storage);
    }
    
    public bool CanOpenSection(int index)
    {
        if (!SectionExists(index)) return false;
        if (GetFsHeader(index).EncryptionType == NcaEncryptionType.None) return true;

        int keyRevision = Utilities.GetMasterKeyRevision(Header.KeyGeneration);

        if (Header.HasRightsId)
        {
            return KeySet.ExternalKeySet.Contains(new RightsId(Header.RightsId)) &&
                   !KeySet.TitleKeks[keyRevision].IsZeros();
        }

        return !KeySet.KeyAreaKeys[keyRevision][Header.KeyAreaKeyIndex].IsZeros();
    }
    
    private IStorage OpenSectionStorage(int index)
    {
        if (!SectionExists(index)) throw new ArgumentException(string.Format(Messages.NcaSectionMissing, index), nameof(index));

        long offset = Header.GetSectionStartOffset(index);
        long size = Header.GetSectionSize(index);

        BaseStorage.GetSize(out long ncaStorageSize).ThrowIfFailure();

        NcaFsHeader fsHeader = Header.GetFsHeader(index);

        if (fsHeader.ExistsSparseLayer())
        {
            ref NcaSparseInfo sparseInfo = ref fsHeader.GetSparseInfo();

            Unsafe.SkipInit(out BucketTree.Header header);
            sparseInfo.MetaHeader.ItemsRo.CopyTo(SpanHelpers.AsByteSpan(ref header));
            header.Verify().ThrowIfFailure();

            var sparseStorage = new SparseStorage();

            if (header.EntryCount == 0)
            {
                sparseStorage.Initialize(size);
            }
            else
            {
                long dataSize = sparseInfo.GetPhysicalSize();

                if (!IsSubRange(sparseInfo.PhysicalOffset, dataSize, ncaStorageSize))
                {
                    throw new InvalidDataException($"Section offset (0x{offset:x}) and length (0x{size:x}) fall outside the total NCA length (0x{ncaStorageSize:x}).");
                }

                IStorage baseStorage = BaseStorage.Slice(sparseInfo.PhysicalOffset, dataSize);
                baseStorage.GetSize(out long baseStorageSize).ThrowIfFailure();

                long metaOffset = sparseInfo.MetaOffset;
                long metaSize = sparseInfo.MetaSize;

                if (metaOffset - sparseInfo.PhysicalOffset + metaSize > baseStorageSize)
                    ResultFs.NcaBaseStorageOutOfRangeB.Value.ThrowIfFailure();

                IStorage metaStorageEncrypted = baseStorage.Slice(metaOffset, metaSize);

                ulong upperCounter = sparseInfo.MakeAesCtrUpperIv(new NcaAesCtrUpperIv(fsHeader.Counter)).Value;
                IStorage metaStorage = OpenAesCtrStorage(metaStorageEncrypted, index, sparseInfo.PhysicalOffset + metaOffset, upperCounter);

                long nodeOffset = 0;
                long nodeSize = IndirectStorage.QueryNodeStorageSize(header.EntryCount);
                // ReSharper disable once UselessBinaryOperation
                long entryOffset = nodeOffset + nodeSize;
                long entrySize = IndirectStorage.QueryEntryStorageSize(header.EntryCount);

                using var nodeStorage = new ValueSubStorage(metaStorage, nodeOffset, nodeSize);
                using var entryStorage = new ValueSubStorage(metaStorage, entryOffset, entrySize);

                sparseStorage.Initialize(new ArrayPoolMemoryResource(), in nodeStorage, in entryStorage, header.EntryCount).ThrowIfFailure();

                using var dataStorage = new ValueSubStorage(baseStorage, 0, sparseInfo.GetPhysicalSize());
                sparseStorage.SetDataStorage(in dataStorage);
            }

            return sparseStorage;
        }

        if (!IsSubRange(offset, size, ncaStorageSize))
        {
            throw new InvalidDataException(
                $"Section offset (0x{offset:x}) and length (0x{size:x}) fall outside the total NCA length (0x{ncaStorageSize:x}).");
        }

        return BaseStorage.Slice(offset, size);
    }
    
    private byte[] GetContentKey(NcaKeyType type)
    {
        return Header.HasRightsId ? GetDecryptedTitleKey() : GetDecryptedKey((int)type);
    }
    
    private byte[] GetDecryptedKey(int index)
    {
        if (index < 0 || index > 3) throw new ArgumentOutOfRangeException(nameof(index));

        // Handle old NCA0s that use different key area encryption
        if (Header.FormatVersion == NcaVersion.Nca0FixedKey || Header.FormatVersion == NcaVersion.Nca0RsaOaep)
        {
            return GetDecryptedKeyAreaNca0().AsSpan(0x10 * index, 0x10).ToArray();
        }

        int keyRevision = Utilities.GetMasterKeyRevision(Header.KeyGeneration);
        byte[] keyAreaKey = KeySet.KeyAreaKeys[keyRevision][Header.KeyAreaKeyIndex].DataRo.ToArray();

        if (keyAreaKey.IsZeros())
        {
            string keyName = $"key_area_key_{KakNames[Header.KeyAreaKeyIndex]}_{keyRevision:x2}";
            throw new MissingKeyException("Unable to decrypt NCA section.", keyName, KeyType.Common);
        }

        byte[] encryptedKey = Header.GetEncryptedKey(index).ToArray();
        byte[] decryptedKey = new byte[Aes.KeySize128];

        Aes.DecryptEcb128(encryptedKey, decryptedKey, keyAreaKey);

        return decryptedKey;
    }
    
    private byte[] GetDecryptedKeyAreaNca0()
    {
        if (Nca0KeyArea != null)
            return Nca0KeyArea;

        if (Header.FormatVersion == NcaVersion.Nca0FixedKey)
        {
            Nca0KeyArea = Header.GetKeyArea().ToArray();
        }
        else if (Header.FormatVersion == NcaVersion.Nca0RsaOaep)
        {
            Span<byte> keyArea = Header.GetKeyArea();
            byte[] decKeyArea = new byte[0x100];

            if (CryptoOld.DecryptRsaOaep(keyArea, decKeyArea, KeySet.BetaNca0KeyAreaKeyParams, out _))
            {
                Nca0KeyArea = decKeyArea;
            }
            else
            {
                throw new InvalidDataException("Unable to decrypt NCA0 key area.");
            }
        }
        else
        {
            throw new NotSupportedException();
        }

        return Nca0KeyArea;
    }
    
    private byte[] GetDecryptedTitleKey()
    {
        int keyRevision = Utilities.GetMasterKeyRevision(Header.KeyGeneration);
        byte[] titleKek = KeySet.TitleKeks[keyRevision].DataRo.ToArray();

        var rightsId = new RightsId(Header.RightsId);

        if (KeySet.ExternalKeySet.Get(rightsId, out AccessKey accessKey).IsFailure())
        {
            throw new MissingKeyException("Missing NCA title key.", rightsId.ToString(), KeyType.Title);
        }

        if (titleKek.IsZeros())
        {
            string keyName = $"titlekek_{keyRevision:x2}";
            throw new MissingKeyException("Unable to decrypt title key.", keyName, KeyType.Common);
        }

        byte[] encryptedKey = accessKey.Value.ToArray();
        byte[] decryptedKey = new byte[Aes.KeySize128];

        Aes.DecryptEcb128(encryptedKey, decryptedKey, titleKek);

        return decryptedKey;
    }
    
    // ReSharper restore UnusedParameter.Local
    private IStorage OpenAesCtrStorage(IStorage baseStorage, int index, long offset, ulong upperCounter)
    {
        byte[] key = GetContentKey(NcaKeyType.AesCtr);
        byte[] counter = Aes128CtrStorage.CreateCounter(upperCounter, Header.GetSectionStartOffset(index));

        var aesStorage = new Aes128CtrStorage(baseStorage, key, offset, counter, true);
        return new CachedStorage(aesStorage, 0x4000, 4, true);
    }
    
    private IStorage OpenAesCtrExStorage(IStorage baseStorage, int index, bool decrypting)
    {
        NcaFsHeader fsHeader = GetFsHeader(index);
        NcaFsPatchInfo info = fsHeader.GetPatchInfo();

        long sectionOffset = Header.GetSectionStartOffset(index);
        long sectionSize = Header.GetSectionSize(index);

        long bktrOffset = info.RelocationTreeOffset;
        long bktrSize = sectionSize - bktrOffset;
        long dataSize = info.RelocationTreeOffset;

        byte[] key = GetContentKey(NcaKeyType.AesCtr);
        byte[] counter = Aes128CtrStorage.CreateCounter(fsHeader.Counter, bktrOffset + sectionOffset);
        byte[] counterEx = Aes128CtrStorage.CreateCounter(fsHeader.Counter, sectionOffset);

        IStorage bucketTreeData;
        IStorage outputBucketTreeData;

        if (decrypting)
        {
            bucketTreeData = new CachedStorage(new Aes128CtrStorage(baseStorage.Slice(bktrOffset, bktrSize), key, counter, true), 4, true);
            outputBucketTreeData = bucketTreeData;
        }
        else
        {
            bucketTreeData = baseStorage.Slice(bktrOffset, bktrSize);
            outputBucketTreeData = new CachedStorage(new Aes128CtrStorage(baseStorage.Slice(bktrOffset, bktrSize), key, counter, true), 4, true);
        }

        var encryptionBucketTreeData = new SubStorage(bucketTreeData,
            info.EncryptionTreeOffset - bktrOffset, sectionSize - info.EncryptionTreeOffset);

        var cachedBucketTreeData = new CachedStorage(encryptionBucketTreeData, IndirectStorage.NodeSize, 6, true);

        var treeHeader = new BucketTree.Header();
        info.EncryptionTreeHeader.CopyTo(SpanHelpers.AsByteSpan(ref treeHeader));
        long nodeStorageSize = AesCtrCounterExtendedStorage.QueryNodeStorageSize(treeHeader.EntryCount);
        long entryStorageSize = AesCtrCounterExtendedStorage.QueryEntryStorageSize(treeHeader.EntryCount);

        var tableNodeStorage = new SubStorage(cachedBucketTreeData, 0, nodeStorageSize);
        var tableEntryStorage = new SubStorage(cachedBucketTreeData, nodeStorageSize, entryStorageSize);

        IStorage decStorage = new Aes128CtrExStorage(baseStorage.Slice(0, dataSize), tableNodeStorage,
            tableEntryStorage, treeHeader.EntryCount, key, counterEx, true);

        return new ConcatenationStorage(new[] { decStorage, outputBucketTreeData }, true);
    }
    
    // ReSharper disable UnusedParameter.Local
    private IStorage OpenAesXtsStorage(IStorage baseStorage, int index, bool decrypting)
    {
        const int sectorSize = 0x200;

        byte[] key0 = GetContentKey(NcaKeyType.AesXts0);
        byte[] key1 = GetContentKey(NcaKeyType.AesXts1);

        // todo: Handle xts for nca version 3
        return new CachedStorage(new Aes128XtsStorage(baseStorage, key0, key1, sectorSize, true, decrypting), 2, true);
    }

    internal IStorage OpenRawStorage(int index, bool openEncrypted = false)
    {
        if (Header.IsNca0())
            return OpenNca0RawStorage(index, openEncrypted);

        IStorage storage = OpenSectionStorage(index);

        if (IsEncrypted == openEncrypted)
        {
            return storage;
        }

        IStorage decryptedStorage = OpenDecryptedStorage(storage, index, !openEncrypted);

        return decryptedStorage;
    }

    private IStorage OpenNca0RawStorage(int index, bool openEncrypted)
    {
        if (!SectionExists(index)) throw new ArgumentException(string.Format(Messages.NcaSectionMissing, index), nameof(index));

        long offset = Header.GetSectionStartOffset(index) - 0x400;
        long size = Header.GetSectionSize(index);

        IStorage bodyStorage = OpenNca0BodyStorage(openEncrypted);

        bodyStorage.GetSize(out long baseSize).ThrowIfFailure();

        if (!IsSubRange(offset, size, baseSize))
        {
            throw new InvalidDataException(
                $"Section offset (0x{offset + 0x400:x}) and length (0x{size:x}) fall outside the total NCA length (0x{baseSize + 0x400:x}).");
        }

        return new SubStorage(bodyStorage, offset, size);
    }

    private IStorage OpenNca0BodyStorage(bool openEncrypted)
    {
        // NCA0 encrypts the entire NCA body using AES-XTS instead of
        // using different encryption types and IVs for each section.
        Assert.SdkEqual(0, Header.Version);

        if (openEncrypted == IsEncrypted)
        {
            return GetRawStorage();
        }

        if (Nca0TransformedBody != null)
            return Nca0TransformedBody;

        byte[] key0 = GetContentKey(NcaKeyType.AesXts0);
        byte[] key1 = GetContentKey(NcaKeyType.AesXts1);

        Nca0TransformedBody = new CachedStorage(new Aes128XtsStorage(GetRawStorage(), key0, key1, HeaderSectorSize, true, !openEncrypted), 1, true);
        return Nca0TransformedBody;

        IStorage GetRawStorage()
        {
            BaseStorage.GetSize(out long ncaSize).ThrowIfFailure();
            return BaseStorage.Slice(0x400, ncaSize - 0x400);
        }
    }
    
    private IStorage OpenDecryptedStorage(IStorage baseStorage, int index, bool decrypting)
    {
        NcaFsHeader header = GetFsHeader(index);

        return header.EncryptionType switch
        {
            NcaEncryptionType.None => baseStorage,
            NcaEncryptionType.AesXts => OpenAesXtsStorage(baseStorage, index, decrypting),
            NcaEncryptionType.AesCtr => OpenAesCtrStorage(baseStorage, index, Header.GetSectionStartOffset(index), header.Counter),
            NcaEncryptionType.AesCtrEx => OpenAesCtrExStorage(baseStorage, index, decrypting),
            _ => throw new NotSupportedException("The encryption type is not supported.")
        };
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
#endif
