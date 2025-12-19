using LibHac.Fs;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibHac.FsSystem;

/// <summary>
/// A storage which uses a relocation tree for mapping virtualized offsets to their physical offset on disk when reading data.
/// </summary>
public class IndirectStorage2 : Storage2
{
    /// <summary>
    /// The definition for an <see cref="IndirectStorage2"/> bucket tree entry.
    /// </summary>
    /// <remarks>See: https://switchbrew.org/wiki/NCA#RomFs_Patching for more information.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Entry : IBucketTreeEntry
    {
        public long VirtualOffset;
        public long PhysicalOffset;
        public int StorageIndex;

        long IBucketTreeEntry.Offset => VirtualOffset;
        
        public override string ToString()
        {
            return $"(VirtualOffset={VirtualOffset}, PhysicalOffset={PhysicalOffset}, StorageIndex={StorageIndex})";
        }
    }

    public override long Length => throw new NotImplementedException();
    
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