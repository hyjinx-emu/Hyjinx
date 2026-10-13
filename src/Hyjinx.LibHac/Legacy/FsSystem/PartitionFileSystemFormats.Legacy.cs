#if IS_LEGACY_ENABLED

using System;
using System.Runtime.InteropServices;

namespace LibHac.FsSystem.Impl;

partial struct PartitionFileSystemFormat : IPartitionFileSystemFormat
{
    public static ReadOnlySpan<byte> VersionSignature => "PFS0"u8;
    public static uint EntryNameLengthMax => PathTool.EntryNameLengthMax;
    public static uint FileDataAlignmentSize => 0x20;
    public static Result ResultSignatureVerificationFailed => ResultFs.PartitionSignatureVerificationFailed.Value;

    partial struct PartitionEntry : IPartitionFileSystemEntry
    {
        readonly long IPartitionFileSystemEntry.Offset => Offset;
        readonly long IPartitionFileSystemEntry.Size => Size;
        readonly int IPartitionFileSystemEntry.NameOffset => NameOffset;
    }

    partial struct PartitionFileSystemHeaderImpl : IPartitionFileSystemHeader
    {
        public readonly ReadOnlySpan<byte> Signature
        {
            get
            {
                ReadOnlySpan<byte> span = _signature.ItemsRo;
                return MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(span), span.Length);
            }
        }

        readonly int IPartitionFileSystemHeader.EntryCount => EntryCount;
        readonly int IPartitionFileSystemHeader.NameTableSize => NameTableSize;
    }
}

partial struct Sha256PartitionFileSystemFormat : IPartitionFileSystemFormat
{
    public static ReadOnlySpan<byte> VersionSignature => "HFS0"u8;
    public static uint EntryNameLengthMax => PathTool.EntryNameLengthMax;
    public static uint FileDataAlignmentSize => 0x200;
    public static Result ResultSignatureVerificationFailed => ResultFs.Sha256PartitionSignatureVerificationFailed.Value;

    partial struct PartitionEntry : IPartitionFileSystemEntry
    {
        readonly long IPartitionFileSystemEntry.Offset => Offset;
        readonly long IPartitionFileSystemEntry.Size => Size;
        readonly int IPartitionFileSystemEntry.NameOffset => NameOffset;
    }
}

#endif