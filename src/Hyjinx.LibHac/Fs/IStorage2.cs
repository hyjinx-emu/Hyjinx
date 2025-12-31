using System;

namespace LibHac.Fs;

/// <summary>
/// Provides an interface for asynchronously reading a sequence of bytes within a region of storage.
/// </summary>
public interface IStorage2 : IDisposable
{
    /// <summary>
    /// Gets the size region.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <remarks>NOTE: The buffer used determines how much data will be read from the storage as there is no result from this operation indicating how much data was read.</remarks>
    /// <param name="offset">The zero-based offset which to seek.</param>
    /// <param name="buffer">The buffer which should receive the data.</param>
    void Read(long offset, Span<byte> buffer);
}