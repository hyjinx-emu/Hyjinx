using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;

namespace LibHac.Tools.Fs;

public class XciPartition : Sha256PartitionFileSystem
{
    public long Offset { get; internal set; }
    public Validity HashValidity { get; set; } = Validity.Unchecked;

    public XciPartition(IStorage storage)
    {
        Initialize(storage).ThrowIfFailure();
    }
}