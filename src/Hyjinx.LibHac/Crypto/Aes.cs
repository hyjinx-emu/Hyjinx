// ReSharper disable AssignmentIsFullyDiscarded
using System;
using System.Runtime.CompilerServices;
using LibHac.Crypto.Impl;
using AesNi = System.Runtime.Intrinsics.X86.Aes;

namespace LibHac.Crypto;

internal static partial class Aes
{
    public const int KeySize128 = 0x10;
    public const int BlockSize = 0x10;

    public static bool IsAesNiSupported()
    {
        return AesNi.IsSupported;
    }
    
    public static ICipher CreateCbcDecryptor(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, bool preferDotNetCrypto = false)
    {
        if (IsAesNiSupported() && !preferDotNetCrypto)
        {
            return new AesCbcDecryptorNi(key, iv);
        }

        return new AesCbcDecryptor(key, iv);
    }

    public static int DecryptCbc128(ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> iv, bool preferDotNetCrypto = false)
    {
        if (IsAesNiSupported() && !preferDotNetCrypto)
        {
            Unsafe.SkipInit(out AesCbcModeNi cipherNi);

            cipherNi.Initialize(key, iv, true);
            return cipherNi.Decrypt(input, output);
        }

        ICipher cipher = CreateCbcDecryptor(key, iv, preferDotNetCrypto);

        return cipher.Transform(input, output);
    }
}
