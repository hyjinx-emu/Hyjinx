using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;

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
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="header">The header.</param>
    protected Xci(XciHeader header)
    {
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

public class Xci1 : Xci
{
    internal IStorage BaseStorage { get; }
    private object InitLocker { get; } = new object();
    private XciPartition RootPartition { get; set; }

    public Xci1(IStorage storage)
        : base(new XciHeader(storage.AsStream()))
    {
        BaseStorage = storage;

        if (Header.HasInitialData)
        {
            BaseStorage = storage.Slice(0x1000);
        }
    }

    public override bool HasPartition(XciPartitionType type)
    {
        if (type == XciPartitionType.Root)
            return true;

        return GetRootPartition().FileExists("/" + type.GetFileName());
    }

    public override IFileSystem OpenPartition(XciPartitionType type)
    {
        XciPartition root = GetRootPartition();
        if (type == XciPartitionType.Root)
            return root;
        string partitionFileName = $"/{type.GetFileName()}";

        using var partitionFile = new UniqueRef<IFile>();
        root.OpenFile(ref partitionFile.Ref, partitionFileName.ToU8Span(), OpenMode.Read).ThrowIfFailure();
        return new XciPartition(partitionFile.Release().AsStorage());
    }

    private XciPartition GetRootPartition()
    {
        if (RootPartition != null)
            return RootPartition;

        InitializeRootPartition();

        return RootPartition;
    }

    private void InitializeRootPartition()
    {
        lock (InitLocker)
        {
            if (RootPartition != null)
                return;

            IStorage rootStorage = BaseStorage.Slice(Header.RootPartitionOffset);

            RootPartition = new XciPartition(rootStorage)
            {
                Offset = Header.RootPartitionOffset,
                HashValidity = Header.PartitionFsHeaderValidity
            };
        }
    }
}