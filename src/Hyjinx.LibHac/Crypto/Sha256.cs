using Org.BouncyCastle.Crypto.Digests;
using System;

namespace LibHac.Crypto;

/// <summary>
/// Represents the SHA-256 hashing algorithm.
/// </summary>
public class Sha256 : IHash
{
    /// <summary>
    /// Defines the size of the digest.
    /// </summary>
    public const int DigestSize = 0x20;

    private readonly Sha256Digest sha256 = new();

    /// <summary>
    /// Generates a SHA-256 hash for the data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="hashBuffer">The buffer to receive the hash.</param>
    public static void GenerateSha256Hash(ReadOnlySpan<byte> data, Span<byte> hashBuffer)
    {
        var sha256 = new Sha256();

        sha256.Initialize();
        sha256.Update(data);
        sha256.GetHash(hashBuffer);
    }

    public void Initialize()
    {
        sha256.Reset();
    }

    public void Update(ReadOnlySpan<byte> data)
    {
        sha256.BlockUpdate(data);
    }

    public void GetHash(Span<byte> hashBuffer)
    {
        sha256.DoFinal(hashBuffer);
    }
}