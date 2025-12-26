using LibHac.Fs;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LibHac.FsSystem;

/// <summary>
/// An <see cref="IStorage2"/> which uses a relocation tree for mapping virtualized offsets to their physical offset on disk.
/// </summary>
/// <remarks>This type of storage is typically used when patching one archive with contents from another archive.</remarks>
public class IndirectStorage2 : Storage2
{
    private readonly IStorage2[] _storages;
    private readonly BucketTree2<Entry> _relocationTree;
    
    /// <summary>
    /// The definition for an <see cref="IndirectStorage2"/> bucket tree entry.
    /// </summary>
    /// <remarks>See: https://switchbrew.org/wiki/NCA#RomFs_Patching for more information.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Entry : IBucketTreeEntry
    {
        /// <summary>
        /// The virtual offset.
        /// </summary>
        public long VirtualOffset { get; init; }
        
        /// <summary>
        /// The physical offset.
        /// </summary>
        public long PhysicalOffset { get; init; }
        
        /// <summary>
        /// The zero-based storage index to use when combining the data.
        /// </summary>
        public int StorageIndex { get; init; }

        long IBucketTreeEntry.Offset => VirtualOffset;
        
        public override string ToString()
        {
            return $"{{ VirtualOffset={VirtualOffset}, PhysicalOffset={PhysicalOffset}, StorageIndex={StorageIndex} }}";
        }
    }

    private IndirectStorage2(IStorage2[] storages, BucketTree2<Entry> relocationTree)
    {
        _storages = storages;
        _relocationTree = relocationTree;
    }

    /// <summary>
    /// Creates an <see cref="IndirectStorage2"/> instance.
    /// </summary>
    /// <remarks>Be advised, the <paramref name="storages"/> provided must match the expected storage indices based on data within the relocation tree.</remarks>
    /// <param name="storages">The storages comprising the underlying storage instances.</param>
    /// <param name="relocationTreeStorage">The storage section containing the relocation tree.</param>
    /// <param name="header">The header for the bucket tree.</param>
    /// <returns>The new instance.</returns>
    /// <exception cref="ArgumentException">The header contains invalid data.</exception>
    public static IndirectStorage2 Create(IStorage2[] storages, IStorage2 relocationTreeStorage, BucketTreeHeader header)
    {
        ArgumentNullException.ThrowIfNull(storages);
        
        var relocationTree = BucketTree2<Entry>.Create(relocationTreeStorage,
            new BucketTreeDefinition
            {
                Header = header,
                Length = relocationTreeStorage.Size
            });
        
        // We cannot verify the storages are provided correctly, but we can at least verify the number provided matches.
        var storagesRequired = relocationTree.Max(o => o.Value.StorageIndex) + 1; // For zero-based index.
        if (storagesRequired != storages.Length)
        {
            throw new ArgumentException(
                $"The number of storages provided did not match the expected number of storages defined within the relocation tree. Expected={storagesRequired}, Actual={storages.Length}",
                nameof(storages));
        }

        return new IndirectStorage2(storages, relocationTree);
    }
    
    public override long Size => throw new NotImplementedException();
    
    public override long Position => throw new NotImplementedException();
    
    public override int Read(Span<byte> buffer)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }
}