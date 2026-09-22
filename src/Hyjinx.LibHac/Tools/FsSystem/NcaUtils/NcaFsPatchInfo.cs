using LibHac.FsSystem;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes patch section data for an Nca header.
/// </summary>
/// <remarks>For more information, see: https://switchbrew.org/wiki/NCA#PatchInfo</remarks>
public class NcaFsPatchInfo
{
    private readonly Memory<byte> data;

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="data">The patch info data.</param>
    public NcaFsPatchInfo(Memory<byte> data)
    {
        this.data = data;
    }

    private ref NcaFsPatchInfoStruct Data => ref Unsafe.As<byte, NcaFsPatchInfoStruct>(ref data.Span[0]);

    /// <summary>
    /// The relocation tree offset.
    /// </summary>
    public long RelocationTreeOffset
    {
        get => Data.RelocationTreeOffset;
        set => Data.RelocationTreeOffset = value;
    }

    /// <summary>
    /// The relocation tree size.
    /// </summary>
    public long RelocationTreeSize
    {
        get => Data.RelocationTreeSize;
        set => Data.RelocationTreeSize = value;
    }

    /// <summary>
    /// The raw relocation tree header.
    /// </summary>
    /// <remarks>Please use the <see cref="GetRelocationTreeHeader"/> method whenever possible rather than interacting with the raw memory bytes.</remarks>
    Memory<byte> RelocationTreeHeader
    {
        get => data.Slice(0x10, 0x10);
        set
        {
            if (value.Length != 0x10)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(data.Slice(0x10, 0x10));
        }
    }

    /// <summary>
    /// The encryption tree offset.
    /// </summary>
    public long EncryptionTreeOffset
    {
        get => Data.EncryptionTreeOffset;
        set => Data.EncryptionTreeOffset = value;
    }

    /// <summary>
    /// The encryption tree size.
    /// </summary>
    public long EncryptionTreeSize
    {
        get => Data.EncryptionTreeSize;
        set => Data.EncryptionTreeSize = value;
    }

    /// <summary>
    /// The raw encryption tree header.
    /// </summary>
    /// <remarks>Please use the <see cref="GetEncryptionTreeHeader"/> method whenever possible rather than interacting with the raw memory bytes.</remarks>
    public Memory<byte> EncryptionTreeHeader
    {
        get => data.Slice(0x30, 0x10);
        set
        {
            if (value.Length != 0x10)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(data.Slice(0x30, 0x10));
        }
    }

    /// <summary>
    /// Gets the encryption tree header reference.
    /// </summary>
    /// <returns>The <see cref="BucketTreeHeader"/>.</returns>
    public ref BucketTreeHeader GetEncryptionTreeHeader()
    {
        return ref MemoryMarshal.Cast<byte, BucketTreeHeader>(EncryptionTreeHeader.Span)[0];
    }

    /// <summary>
    /// Gets the relocation tree header reference.
    /// </summary>
    /// <returns>The <see cref="BucketTreeHeader"/>.</returns>
    public ref BucketTreeHeader GetRelocationTreeHeader()
    {
        return ref MemoryMarshal.Cast<byte, BucketTreeHeader>(RelocationTreeHeader.Span)[0];
    }
}