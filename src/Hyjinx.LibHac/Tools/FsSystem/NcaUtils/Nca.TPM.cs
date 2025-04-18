#if IS_TPM_BYPASS_ENABLED
#pragma warning disable CS0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Spl;
using LibHac.Tools.Crypto;
using System;
using System.IO;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class Nca
{
    private KeySet KeySet { get; }
    
    public Nca(KeySet keySet, IStorage storage)
    {
        KeySet = keySet;
        BaseStorage = storage;
        Header = new NcaHeader(keySet, storage);
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
}

#pragma warning restore CS0618 // Type or member is obsolete
#endif
