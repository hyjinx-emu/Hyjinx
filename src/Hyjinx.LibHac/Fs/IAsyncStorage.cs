using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Fs;

/// <summary>
/// Provides an interface for asynchronously reading a sequence of bytes within a region of storage.
/// </summary>
/// <remarks>Based on nnSdk 14.3.0 (FS 14.1.0)</remarks>
public interface IAsyncStorage : IStorage, IAsyncDisposable
{
    /// <summary>
    /// Gets the length of the storage region.
    /// </summary>
    long Length { get; }
    
    /// <summary>
    /// Gets the current position within the storage region.
    /// </summary>
    long Position { get; }

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="buffer">The buffer which should receive the data.</param>
    /// <returns>The number of bytes read. This will typically match the buffer size, however it may not as the end of the storage region is being reached. A return value of 0 will always occur when the end of the region has been reached.</returns>
    int Read(Span<byte> buffer);
    
    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="buffer">The buffer which should receive the data.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The number of bytes read. This will typically match the buffer size, however it may not as the end of the storage region is being reached. A return value of 0 will always occur when the end of the region has been reached.</returns>
    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Seeks position within the storage.
    /// </summary>
    /// <param name="offset">The zero-based offset which to seek.</param>
    /// <param name="origin">The origin of which the offset should be applied.</param>
    /// <returns>The new position.</returns>
    long Seek(long offset, SeekOrigin origin);
}