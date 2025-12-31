#if IS_LEGACY_ENABLED

using LibHac.Common;
using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

partial class NcaHeader
{
    public Span<byte> Signature1 => _header.Span.Slice(0, 0x100);
    public Span<byte> Signature2 => _header.Span.Slice(0x100, 0x100);

    public TitleVersion SdkVersion
    {
        get => new(Header.SdkVersion);
        set => Header.SdkVersion = value.Version;
    }

    public bool HasRightsId => !RightsId.IsZeros();

    public long GetSectionEndOffset(int index)
    {
        return BlockToOffset(GetSectionEntry(index).EndBlock);
    }
}

#endif