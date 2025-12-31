using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

public class Xci
{
    public XciHeader Header { get; }

    internal IStorage BaseStorage { get; }
    private object InitLocker { get; } = new object();
    private XciPartition RootPartition { get; set; }

    public Xci(IStorage storage)
    {
        BaseStorage = storage;
        Header = new XciHeader(storage.AsStream());

        if (Header.HasInitialData)
        {
            BaseStorage = storage.Slice(0x1000);
        }
    }

    public bool HasPartition(XciPartitionType type)
    {
        if (type == XciPartitionType.Root)
            return true;

        return GetRootPartition().FileExists("/" + type.GetFileName());
    }

    public XciPartition OpenPartition(XciPartitionType type)
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