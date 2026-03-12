using System;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Describes patch section data for an Nca header.
/// </summary>
/// <remarks>For more information, see: https://switchbrew.org/wiki/NCA#PatchInfo</remarks>
public class NcaFsPatchInfo
{
    private readonly Memory<byte> _data;

    /// <summary>
    /// Creates an instance of the class.
    /// </summary>
    /// <param name="data">The patch info data.</param>
    public NcaFsPatchInfo(Memory<byte> data)
    {
        _data = data;
    }

    private ref NcaFsPatchInfoStruct Data => ref Unsafe.As<byte, NcaFsPatchInfoStruct>(ref _data.Span[0]);

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
    public Memory<byte> RelocationTreeHeader => _data.Slice(0x10, 0x10);

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
    public Memory<byte> EncryptionTreeHeader => _data.Slice(0x30, 0x10);
}