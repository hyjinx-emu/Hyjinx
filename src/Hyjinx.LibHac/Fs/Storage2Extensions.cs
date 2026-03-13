using LibHac.Tools.FsSystem;
using System.IO;

namespace LibHac.Fs;

/// <summary>
/// Contains extension methods for the <see cref="IStorage"/> interface.
/// </summary>
public static class Storage2Extensions
{
    /// <summary>
    /// Creates a <see cref="Stream"/> from the storage.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <returns>The new <see cref="Stream"/> instance.</returns>
    public static Stream AsStream2(this IStorage storage)
    {
        return new NxFileStream2(storage);
    }

    /// <summary>
    /// Adapts a <see cref="Stream"/> to an <see cref="IStorage"/> interface.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to adapt.</param>
    /// <param name="leaveOpen"><c>true</c> if <paramref name="stream"/> should be kept open when the storage is disposed, otherwise <c>false</c>.</param>
    /// <returns>A new <see cref="StreamStorage2"/> instance.</returns>
    public static IStorage AsStorage2(this Stream stream, bool leaveOpen = true)
    {
        return StreamStorage2.Create(stream, leaveOpen);
    }

    /// <summary>
    /// Creates a new slice of storage.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="offset">The zero-index offset within the storage.</param>
    /// <param name="length">The length of data within the storage section.</param>
    /// <returns>The new <see cref="IStorage"/> slice.</returns>
    public static IStorage Slice2(this IStorage storage, long offset, long length)
    {
        return SubStorage2.Create(storage, offset, length);
    }
}