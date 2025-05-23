using LibHac.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LibHac.Spl;

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct AccessKey : IEquatable<AccessKey>
{
    private readonly Key128 Key;

    [UnscopedRef] public ReadOnlySpan<byte> Value => SpanHelpers.AsByteSpan(ref this);

    public AccessKey(ReadOnlySpan<byte> bytes)
    {
        Key = new Key128(bytes);
    }

    public override string ToString() => Key.ToString();

    public override bool Equals(object obj) => obj is AccessKey key && Equals(key);
    public bool Equals(AccessKey other) => Key.Equals(other.Key);
    public override int GetHashCode() => Key.GetHashCode();
    public static bool operator ==(AccessKey left, AccessKey right) => left.Equals(right);
    public static bool operator !=(AccessKey left, AccessKey right) => !(left == right);
}