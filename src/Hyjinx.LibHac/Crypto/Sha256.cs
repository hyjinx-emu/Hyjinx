using LibHac.Crypto.Impl;
using System;

namespace LibHac.Crypto;

public class Sha256 : IHash
{
    public const int DigestSize = 0x20;

    private readonly Sha256Impl sha256 = new();
    
    /// <summary>
    /// Creates an uninitialized SHA-256 <see cref="IHash"/> object.
    /// </summary>
    /// <returns> The new uninitialized SHA-256 <see cref="IHash"/> object.</returns>
    internal static Sha256 CreateSha256Generator()
    {
        return new Sha256();
    }

    /// <summary>
    /// Generates a Sha256 hash for the data.
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
        sha256.Initialize();
    }

    public void Update(ReadOnlySpan<byte> data)
    {
        sha256.Update(data);
    }

    public void GetHash(Span<byte> hashBuffer)
    {
        sha256.GetHash(hashBuffer);
    }
}