using LibHac.Diag;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;

namespace LibHac.Crypto;

/// <summary>
/// Represents the HMAC-SHA-256 hashing algorithm.
/// </summary>
internal static class HmacSha256
{
    /// <summary>
    /// Defines the size of the hash.
    /// </summary>
    public const int HashSize = Sha256.DigestSize;

    /// <summary>
    /// Generates an HMAC-SHA-256 for the data.
    /// </summary>
    /// <param name="outMac">The output hash.</param>
    /// <param name="data">The input data.</param>
    /// <param name="key">The key.</param>
    public static void GenerateHmacSha256(Span<byte> outMac, ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        var hmac = new HMac(new Sha256Digest());
        hmac.Init(new KeyParameter(key));
        hmac.BlockUpdate(data);

        var bytesWritten = hmac.DoFinal(outMac);
        Abort.DoAbortUnless(bytesWritten == HashSize);
    }
}