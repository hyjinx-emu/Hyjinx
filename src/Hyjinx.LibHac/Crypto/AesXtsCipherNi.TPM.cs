#if IS_TPM_BYPASS_ENABLED

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using LibHac.Common;
using LibHac.Crypto.Impl;

namespace LibHac.Crypto;

internal class AesXtsEncryptorNi : ICipherWithIv
{
    private AesXtsModeNi _baseCipher;

    public ref Buffer16 Iv => ref Unsafe.As<Vector128<byte>, Buffer16>(ref _baseCipher.Iv);

    public AesXtsEncryptorNi(ReadOnlySpan<byte> key1, ReadOnlySpan<byte> key2, ReadOnlySpan<byte> iv)
    {
        _baseCipher = new AesXtsModeNi();
        _baseCipher.Initialize(key1, key2, iv, false);
    }

    public int Transform(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return _baseCipher.Encrypt(input, output);
    }
}

internal class AesXtsDecryptorNi : ICipherWithIv
{
    private AesXtsModeNi _baseCipher;

    public ref Buffer16 Iv => ref Unsafe.As<Vector128<byte>, Buffer16>(ref _baseCipher.Iv);

    public AesXtsDecryptorNi(ReadOnlySpan<byte> key1, ReadOnlySpan<byte> key2, ReadOnlySpan<byte> iv)
    {
        _baseCipher = new AesXtsModeNi();
        _baseCipher.Initialize(key1, key2, iv, true);
    }

    public int Transform(ReadOnlySpan<byte> input, Span<byte> output)
    {
        return _baseCipher.Decrypt(input, output);
    }
}

#endif
