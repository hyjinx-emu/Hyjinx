using LibHac.Diag;
using System;
using System.Security.Cryptography;

namespace LibHac.Crypto;

internal static class HmacSha256
{
    public const int HashSize = Sha256.DigestSize;

    public static void GenerateHmacSha256(Span<byte> outMac, ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        bool success = HMACSHA256.TryHashData(key, data, outMac, out int bytesWritten);

        Abort.DoAbortUnless(success && bytesWritten == HashSize);
    }
}