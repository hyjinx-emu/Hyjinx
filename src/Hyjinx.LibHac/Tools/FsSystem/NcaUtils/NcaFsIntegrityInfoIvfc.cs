using System;
using System.Runtime.CompilerServices;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tools.FsSystem.NcaUtils;

public struct NcaFsIntegrityInfoIvfc
{
    private readonly Memory<byte> _data;

    public NcaFsIntegrityInfoIvfc(Memory<byte> data)
    {
        _data = data;
    }

    private ref IvfcStruct Data => ref Unsafe.As<byte, IvfcStruct>(ref _data.Span[0]);

    private ref IvfcLevel GetLevelInfo(int index)
    {
        ValidateLevelIndex(index);

        int offset = IvfcStruct.IvfcLevelsOffset + IvfcLevel.IvfcLevelSize * index;
        return ref Unsafe.As<byte, IvfcLevel>(ref _data.Span[offset]);
    }

    public uint Magic
    {
        get => Data.Magic;
        set => Data.Magic = value;
    }

    public int Version
    {
        get => Data.Version;
        set => Data.Version = value;
    }

    public int MasterHashSize
    {
        get => Data.MasterHashSize;
        set => Data.MasterHashSize = value;
    }

    public int LevelCount
    {
        get => Data.LevelCount;
        set => Data.LevelCount = value;
    }

    public Span<byte> SaltSource => _data.Span.Slice(IvfcStruct.SaltSourceOffset, IvfcStruct.SaltSourceSize);
    public Span<byte> MasterHash => _data.Span.Slice(IvfcStruct.MasterHashOffset, MasterHashSize);

    public ref long GetLevelOffset(int index) => ref GetLevelInfo(index).Offset;
    public ref long GetLevelSize(int index) => ref GetLevelInfo(index).Size;
    public ref int GetLevelBlockSize(int index) => ref GetLevelInfo(index).BlockSize;

    private static void ValidateLevelIndex(int index)
    {
        if (index < 0 || index > 6)
        {
            throw new ArgumentOutOfRangeException($"IVFC level index must be between 0 and 6. Actual: {index}");
        }
    }
}
