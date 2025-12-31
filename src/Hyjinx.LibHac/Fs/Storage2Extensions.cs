using LibHac.Tools.FsSystem;
using System.IO;

namespace LibHac.Fs;

/// <summary>
/// Contains extension methods for the <see cref="IStorage2"/> interface.
/// </summary>
public static class Storage2Extensions
{
    /// <summary>
    /// Creates a <see cref="Stream"/> from the storage.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <returns>The new <see cref="Stream"/> instance.</returns>
    public static Stream AsStream(this IStorage2 storage)
    {
        return new NxFileStream2(storage);
    }

    /// <summary>
    /// Creates a new slice of storage.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="offset">The zero-index offset within the storage.</param>
    /// <param name="length">The length of data within the storage section.</param>
    /// <returns>The new <see cref="IStorage2"/> slice.</returns>
    public static IStorage2 Slice2(this IStorage2 storage, long offset, long length)
    {
        return SubStorage2.Create(storage, offset, length);
    }
}