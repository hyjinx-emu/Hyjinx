#if IS_LEGACY_ENABLED

using System;

namespace LibHac.Crypto.Impl;

internal struct AesEcbModeNi
{
    private AesCoreNi _aesCore;

    public void Initialize(ReadOnlySpan<byte> key, bool isDecrypting)
    {
        _aesCore.Initialize(key, isDecrypting);
    }

    public int Encrypt(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return _aesCore.EncryptInterleaved8(input, output);
    }

    public int Decrypt(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return _aesCore.DecryptInterleaved8(input, output);
    }
}

#endif
