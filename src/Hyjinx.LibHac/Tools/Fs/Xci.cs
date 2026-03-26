using LibHac.Fs;
using LibHac.Fs.Fsa;

namespace LibHac.Tools.Fs;

/// <summary>
/// Represents an XCI archive.
/// </summary>
public abstract class Xci
{
    /// <summary>
    /// Gets the header.
    /// </summary>
    public XciHeader Header { get; }

    /// <summary>
    /// Gets the base storage.
    /// </summary>
    internal protected IStorage BaseStorage { get; protected set; }

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="baseStorage">The storage to use.</param>
    /// <param name="header">The header.</param>
    protected Xci(IStorage baseStorage, XciHeader header)
    {
        BaseStorage = baseStorage;
        Header = header;
    }

    /// <summary>
    /// Identifies whether the partition exists.
    /// </summary>
    /// <param name="type">The partition type to check.</param>
    /// <returns><c>true</c> if the partition exists, otherwise <c>false</c>.</returns>
    public abstract bool HasPartition(XciPartitionType type);

    /// <summary>
    /// Opens the partition.
    /// </summary>
    /// <param name="type">The partition type.</param>
    /// <returns>The <see cref="IFileSystem"/> instance.</returns>
    public abstract IFileSystem OpenPartition(XciPartitionType type);
}