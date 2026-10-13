using LibHac.Common.FixedArrays;
using System.Runtime.InteropServices;

namespace LibHac.FsSystem.Impl;

public partial struct PartitionFileSystemFormat
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PartitionEntry
    {
        public long Offset;
        public long Size;
        public int NameOffset;
        public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct PartitionFileSystemHeaderImpl
    {
        private Array4<byte> _signature;
        public int EntryCount;
        public int NameTableSize;
        public uint Reserved;
    }
}

public partial struct Sha256PartitionFileSystemFormat
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PartitionEntry
    {
        public long Offset;
        public long Size;
        public int NameOffset;
        public int HashTargetSize;
        public long HashTargetOffset;
        public Array32<byte> Hash;
    }
}