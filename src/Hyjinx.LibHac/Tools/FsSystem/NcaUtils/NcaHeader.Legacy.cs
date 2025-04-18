#if IS_UNUSED_ENABLED

using LibHac.Common;
using LibHac.Tools.Crypto;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaHeader
{
    public TitleVersion SdkVersion
    {
        get => new TitleVersion(Header.SdkVersion);
        set => Header.SdkVersion = value.Version;
    }
    
    public long GetSectionEndOffset(int index)
    {
        return BlockToOffset(GetSectionEntry(index).EndBlock);
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
