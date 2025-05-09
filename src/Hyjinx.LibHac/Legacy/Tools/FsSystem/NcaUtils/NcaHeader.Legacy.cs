#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Crypto;

// ReSharper disable once CheckNamespace
namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaHeader
{
    public long GetSectionEndOffset(int index)
    {
        return BlockToOffset(GetSectionEntry(index).EndBlock);
    }

    public Span<byte> GetEncryptedKey(int index)
    {
        if (index < 0 || index >= SectionCount)
        {
            throw new ArgumentOutOfRangeException($"Key index must be between 0 and 3. Actual: {index}");
        }

        int offset = KeyAreaOffset + Aes.KeySize128 * index;
        return _header.Span.Slice(offset, Aes.KeySize128);
    }
    
    public Validity VerifySignature1(byte[] modulus)
    {
        return CryptoOld.Rsa2048PssVerify(_header.Span.Slice(0x200, 0x200).ToArray(), Signature1.ToArray(), modulus);
    }

    public Validity VerifySignature2(byte[] modulus)
    {
        return CryptoOld.Rsa2048PssVerify(_header.Span.Slice(0x200, 0x200).ToArray(), Signature2.ToArray(), modulus);
    }
}

#endif
