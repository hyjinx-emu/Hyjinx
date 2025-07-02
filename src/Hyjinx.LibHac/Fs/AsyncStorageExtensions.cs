using LibHac.Tools.FsSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Fs;

/// <summary>
/// Contains extension methods for the <see cref="IAsyncStorage"/> interface.
/// </summary>
public static class AsyncStorageExtensions
{
    /// <summary>
    /// Creates a new slice of storage.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="offset">The zero-index offset within the storage.</param>
    /// <param name="length">The length of data within the storage section.</param>
    /// <returns>The new <see cref="IAsyncStorage"/> slice.</returns>
    public static IAsyncStorage SliceAsAsync(this IAsyncStorage storage, long offset, long length)
    {
        return SubStorage2.Create(storage, offset, length);
    }

    /// <summary>
    /// Reads the data once.
    /// </summary>
    /// <remarks><b>CAUTION! </b>This method will cause random access to the underlying storage as the position is being reset after use.</remarks>
    /// <param name="storage">The storage.</param>
    /// <param name="offset">The zero-index offset within the storage from the beginning of the storage.</param>
    /// <param name="buffer">The buffer which should receive the data.</param>
    /// <returns>The number of bytes read. This will typically match the buffer size, however it may not as the end of the storage region is being reached. A return value of 0 will always occur when the end of the region has been reached.</returns>
    [Obsolete($"This method is no longer supported and is only provided for backwards compatibility. Usage of this method is likely to impact performance, please use the {nameof(ReadOnceAsync)} variant instead.")]
    public static int ReadOnce(this IAsyncStorage storage, long offset, Span<byte> buffer)
    {
        // Grab the starting position so we can move back there before exiting the method.
        long position = storage.Position;
        
        try
        {
            // Do the seek, but do not check position in case the underlying storage has to be repositioned.
            storage.Seek(offset, SeekOrigin.Begin);

            return storage.Read(buffer);
        }
        finally
        {
            // Make sure the stream is repositioned where it started at upon leaving the method.
            storage.Seek(position, SeekOrigin.Begin);
        }
    }
    
    /// <summary>
    /// Reads the data once.
    /// </summary>
    /// <remarks><b>CAUTION! </b>This method will cause random access to the underlying storage as the position is being reset after use.</remarks>
    /// <param name="storage">The storage.</param>
    /// <param name="offset">The zero-index offset within the storage from the beginning of the storage.</param>
    /// <param name="buffer">The buffer which should receive the data.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>The number of bytes read. This will typically match the buffer size, however it may not as the end of the storage region is being reached. A return value of 0 will always occur when the end of the region has been reached.</returns>
    public static async Task<int> ReadOnceAsync(this IAsyncStorage storage, long offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        // Grab the starting position so we can move back there before exiting the method.
        long position = storage.Position;
        
        try
        {
            // Do the seek, but do not check position in case the underlying storage has to be repositioned.
            storage.Seek(offset, SeekOrigin.Begin);

            return await storage.ReadAsync(buffer, cancellationToken);
        }
        finally
        {
            // Make sure the stream is repositioned where it started at upon leaving the method.
            storage.Seek(position, SeekOrigin.Begin);
        }
    }
}