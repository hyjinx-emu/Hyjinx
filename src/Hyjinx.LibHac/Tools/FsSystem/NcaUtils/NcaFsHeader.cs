using LibHac.FsSystem;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

public struct NcaFsHeader
{
    private readonly Memory<byte> _header;

    public NcaFsHeader(Memory<byte> headerData)
    {
        _header = headerData;
    }

    private ref FsHeaderStruct Header => ref Unsafe.As<byte, FsHeaderStruct>(ref _header.Span[0]);

    public short Version
    {
        get => Header.Version;
        set => Header.Version = value;
    }

    public NcaFormatType FormatType
    {
        get => (NcaFormatType)Header.FormatType;
        set => Header.FormatType = (byte)value;
    }

    public NcaHashType HashType
    {
        get => (NcaHashType)Header.HashType;
        set => Header.HashType = (byte)value;
    }

    public NcaEncryptionType EncryptionType
    {
        get => (NcaEncryptionType)Header.EncryptionType;
        set => Header.EncryptionType = (byte)value;
    }

    public NcaFsIntegrityInfoIvfc GetIntegrityInfoIvfc()
    {
        return new NcaFsIntegrityInfoIvfc(_header.Slice(IntegrityInfoOffset, IntegrityInfoSize));
    }

    public NcaFsIntegrityInfoSha256 GetIntegrityInfoSha256()
    {
        return new NcaFsIntegrityInfoSha256(_header.Slice(IntegrityInfoOffset, IntegrityInfoSize));
    }

    public NcaFsPatchInfo GetPatchInfo()
    {
        return new NcaFsPatchInfo(_header.Slice(PatchInfoOffset, PatchInfoSize));
    }

    public bool IsPatchSection()
    {
        return GetPatchInfo().RelocationTreeSize != 0;
    }

    public ref NcaSparseInfo GetSparseInfo()
    {
        return ref MemoryMarshal.Cast<byte, NcaSparseInfo>(_header.Span.Slice(SparseInfoOffset, SparseInfoSize))[0];
    }

    public bool ExistsSparseLayer()
    {
        return GetSparseInfo().Generation != 0;
    }

    public ref NcaCompressionInfo GetCompressionInfo()
    {
        return ref MemoryMarshal.Cast<byte, NcaCompressionInfo>(_header.Span.Slice(CompressionInfoOffset, CompressionInfoSize))[0];
    }

    public bool ExistsCompressionLayer()
    {
        return GetCompressionInfo().TableOffset != 0 && GetCompressionInfo().TableSize != 0;
    }

    public ulong Counter
    {
        get => Header.UpperCounter;
        set => Header.UpperCounter = value;
    }

    public int CounterType
    {
        get => Header.CounterType;
        set => Header.CounterType = value;
    }

    public int CounterVersion
    {
        get => Header.CounterVersion;
        set => Header.CounterVersion = value;
    }
}