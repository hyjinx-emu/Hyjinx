using LibHac.FsSystem;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Provides a view over a memory block containing an NCA FS header.
/// </summary>
public class NcaFsHeader
{
    /// <summary>
    /// The parent header.
    /// </summary>
    public NcaHeader Parent { get; }

    /// <summary>
    /// The raw header data.
    /// </summary>
    /// <remarks>Use caution if modifying the raw data, as it is very easy to invalidate the header.</remarks>
    public Memory<byte> Data { get; }

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="parent">The parent header.</param>
    /// <param name="data">The memory block containing the header data.</param>
    public NcaFsHeader(NcaHeader parent, Memory<byte> data)
    {
        Parent = parent;
        Data = data;
    }

    /// <summary>
    /// The header structure.
    /// </summary>
    protected ref FsHeaderStruct Header => ref Unsafe.As<byte, FsHeaderStruct>(ref Data.Span[0]);

    /// <summary>
    /// The version.
    /// </summary>
    public short Version
    {
        get => Header.Version;
        set => Header.Version = value;
    }

    /// <summary>
    /// The format type.
    /// </summary>
    public NcaFormatType FormatType
    {
        get => (NcaFormatType)Header.FormatType;
        set => Header.FormatType = (byte)value;
    }

    /// <summary>
    /// The hash type.
    /// </summary>
    public NcaHashType HashType
    {
        get => (NcaHashType)Header.HashType;
        set => Header.HashType = (byte)value;
    }

    /// <summary>
    /// The encryption type.
    /// </summary>
    public byte EncryptionType
    {
        get => Header.EncryptionType;
        set => Header.EncryptionType = value;
    }

    /// <summary>
    /// The checksum.
    /// </summary>
    public Memory<byte> Checksum
    {
        get => Data.Slice(IntegrityInfoOffset, IntegrityInfoSize);
        set
        {
            if (value.Length != IntegrityInfoSize)
            {
                throw new ArgumentException("The value is the wrong size.", nameof(value));
            }

            value.CopyTo(Data.Slice(IntegrityInfoOffset, IntegrityInfoSize));
        }
    }

    /// <summary>
    /// Gets the patch info.
    /// </summary>
    /// <returns>The <see cref="NcaFsPatchInfo"/> describing the patch section.</returns>
    public NcaFsPatchInfo GetPatchInfo()
    {
        return new NcaFsPatchInfo(Data.Slice(PatchInfoOffset, PatchInfoSize));
    }

    public bool IsPatchSection()
    {
        return GetPatchInfo().RelocationTreeSize != 0;
    }

    public ref NcaSparseInfo GetSparseInfo()
    {
        return ref MemoryMarshal.Cast<byte, NcaSparseInfo>(Data.Span.Slice(SparseInfoOffset, SparseInfoSize))[0];
    }

    public bool ExistsSparseLayer()
    {
        return GetSparseInfo().Generation != 0;
    }

    public ref NcaCompressionInfo GetCompressionInfo()
    {
        return ref MemoryMarshal.Cast<byte, NcaCompressionInfo>(Data.Span.Slice(CompressionInfoOffset, CompressionInfoSize))[0];
    }

    public bool ExistsCompressionLayer()
    {
        return GetCompressionInfo().TableOffset != 0 && GetCompressionInfo().TableSize != 0;
    }
}