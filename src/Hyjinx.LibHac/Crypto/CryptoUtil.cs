using LibHac.Common;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Crypto;

/// <summary>
/// Contains utility methods used for cryptographic functions.
/// </summary>
public static class CryptoUtil
{
    /// <summary>
    /// Checks the input hash matches the expected hash.
    /// </summary>
    /// <param name="input">The input data to hash.</param>
    /// <param name="expected">The expected hash.</param>
    /// <returns>The <see cref="Validity"/> whether the hash matches.</returns>
    public static Validity CheckSha256Hash(in Span<byte> input, in Span<byte> expected)
    {
        Span<byte> actualHash = stackalloc byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash(input, actualHash);

        // Compare the hash to the expected value.
        if (expected.SequenceEqual(actualHash))
        {
            return Validity.Valid;
        }

        return Validity.Invalid;
    }

    public static bool IsSameBytes(ReadOnlySpan<byte> buffer1, ReadOnlySpan<byte> buffer2, int length)
    {
        if (buffer1.Length < (uint)length || buffer2.Length < (uint)length)
            throw new ArgumentOutOfRangeException(nameof(length));

        return IsSameBytes(ref MemoryMarshal.GetReference(buffer1), ref MemoryMarshal.GetReference(buffer2), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSameBytes(ref byte p1, ref byte p2, int length)
    {
        int result = 0;

        for (int i = 0; i < length; i++)
        {
            result |= Unsafe.Add(ref p1, i) ^ Unsafe.Add(ref p2, i);
        }

        return result == 0;
    }
}