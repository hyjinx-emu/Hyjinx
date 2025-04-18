#if IS_TPM_BYPASS_ENABLED
#pragma warning disable CS0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Spl;
using System;
using System.IO;
using KeyType = LibHac.Common.Keys.KeyType;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca
{
    [Obsolete("This property can no longer be used due to TPM restrictions.")]
    private bool IsEncrypted => Header.IsEncrypted;
    
    private KeySet KeySet { get; }
    
    public Nca(KeySet keySet, IStorage storage)
    {
        KeySet = keySet;
        BaseStorage = storage;
        Header = new NcaHeader(keySet, storage);
    }
    
    private byte[] GetContentKey(NcaKeyType type)
    {
        return Header.HasRightsId ? GetDecryptedTitleKey() : GetDecryptedKey((int)type);
    }
    
    public byte[] GetDecryptedKey(int index)
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
    
    public byte[] GetDecryptedTitleKey()
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
}

#pragma warning restore CS0618 // Type or member is obsolete
#endif
