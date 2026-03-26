#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;

namespace LibHac.Tools.Fs;

public class Xci1 : Xci
{
    private object InitLocker { get; } = new object();
    private XciPartition RootPartition { get; set; }

    public new XciHeader1 Header => (XciHeader1)base.Header;

    public Xci1(IStorage storage)
        : base(storage, new XciHeader1(storage.AsStream()))
    {
        if (Header.HasInitialData)
        {
            BaseStorage = BaseStorage.Slice(0x1000);
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

#endif