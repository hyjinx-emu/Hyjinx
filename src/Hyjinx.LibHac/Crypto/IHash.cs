using System;

namespace LibHac.Crypto;

/// <summary>
/// Identifies a hashing algorithm.
/// </summary>
public interface IHash
{
    /// <summary>
    /// Initializes the algorithm.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Updates the data for the hash.
    /// </summary>
    /// <param name="data"></param>
    void Update(ReadOnlySpan<byte> data);

    /// <summary>
    /// Calculates and retrieves the hash of the data.
    /// </summary>
    /// <param name="hashBuffer">The buffer which should receive the hash.</param>
    void GetHash(Span<byte> hashBuffer);
}