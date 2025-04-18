using System;
using LibHac.Crypto.Impl;

namespace LibHac.Crypto;

internal class AesCbcEncryptor : ICipher
{
    private AesCbcMode _baseCipher;

    public AesCbcEncryptor(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv)
    {
        _baseCipher = new AesCbcMode();
        _baseCipher.Initialize(key, iv, false);
    }

    public int Transform(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return _baseCipher.Encrypt(input, output);
    }
}

internal class AesCbcDecryptor : ICipher
{
    private AesCbcMode _baseCipher;

    public AesCbcDecryptor(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv)
    {
        _baseCipher = new AesCbcMode();
        _baseCipher.Initialize(key, iv, true);
    }

    public int Transform(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return _baseCipher.Decrypt(input, output);
    }
}
