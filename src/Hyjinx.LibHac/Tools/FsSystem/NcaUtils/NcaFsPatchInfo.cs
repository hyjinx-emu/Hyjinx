using System;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

public struct NcaFsPatchInfo
{
    private readonly Memory<byte> _data;

    public NcaFsPatchInfo(Memory<byte> data)
    {
        _data = data;
    }

    private ref NcaFsPatchInfoStruct Data => ref Unsafe.As<byte, NcaFsPatchInfoStruct>(ref _data.Span[0]);

    public long RelocationTreeOffset
    {
        get => Data.RelocationTreeOffset;
        set => Data.RelocationTreeOffset = value;
    }

    public long RelocationTreeSize
    {
        get => Data.RelocationTreeSize;
        set => Data.RelocationTreeSize = value;
    }

    public long EncryptionTreeOffset
    {
        get => Data.EncryptionTreeOffset;
        set => Data.EncryptionTreeOffset = value;
    }

    public long EncryptionTreeSize
    {
        get => Data.EncryptionTreeSize;
        set => Data.EncryptionTreeSize = value;
    }

    public Span<byte> RelocationTreeHeader => _data.Span.Slice(0x10, 0x10);
    public Span<byte> EncryptionTreeHeader => _data.Span.Slice(0x30, 0x10);
}
