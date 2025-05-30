using LibHac.Common;
using LibHac.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Kvdb;

public struct BoundedString<TSize> where TSize : unmanaged
{
    private TSize _string;

    [UnscopedRef] public Span<byte> Get() => SpanHelpers.AsByteSpan(ref _string);

    public int GetLength() =>
        StringUtils.GetLength(SpanHelpers.AsReadOnlyByteSpan(in _string), Unsafe.SizeOf<TSize>());
}

[StructLayout(LayoutKind.Sequential, Size = 768)]
internal struct Size768 { }