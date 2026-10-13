using LibHac.Util.Impl;
using System;
using System.Text;

namespace LibHac.Util;

public static partial class StringUtils
{
    public static int Copy(Span<byte> dest, ReadOnlySpan<byte> source)
    {
        int maxLen = Math.Min(dest.Length, source.Length);

        int i;
        for (i = 0; i < maxLen && source[i] != 0; i++)
            dest[i] = source[i];

        if (i < dest.Length)
        {
            dest[i] = 0;
        }

        return i;
    }

    public static int Copy(Span<byte> dest, ReadOnlySpan<byte> source, int maxLen)
    {
        int maxLenLocal = Math.Min(Math.Min(dest.Length, source.Length), maxLen);

        int i;
        for (i = 0; i < maxLenLocal && source[i] != 0; i++)
            dest[i] = source[i];

        if (i < dest.Length)
        {
            dest[i] = 0;
        }

        return i;
    }

    public static int Strlcpy(Span<byte> dest, ReadOnlySpan<byte> source, int maxLen)
    {
        int maxLenLocal = Math.Min(Math.Min(dest.Length, source.Length), maxLen);

        int i = 0;
        if (maxLenLocal > 0)
        {
            for (; i < maxLenLocal - 1 && source[i] != 0; i++)
            {
                dest[i] = source[i];
            }

            dest[i] = 0;
        }

        while (i < source.Length && source[i] != 0)
        {
            i++;
        }

        return i;
    }

    public static int GetLength(ReadOnlySpan<byte> s)
    {
        int i = 0;

        while (i < s.Length && s[i] != 0)
        {
            i++;
        }

        return i;
    }

    public static int GetLength(ReadOnlySpan<byte> s, int maxLen)
    {
        int i = 0;

        while (i < maxLen && i < s.Length && s[i] != 0)
        {
            i++;
        }

        return i;
    }

    public static int Compare(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2)
    {
        int i = 0;

        while (true)
        {
            int c1 = ((uint)i < (uint)s1.Length ? s1[i] : 0);
            int c2 = ((uint)i < (uint)s2.Length ? s2[i] : 0);

            if (c1 != c2)
                return c1 - c2;

            if (c1 == 0)
                return 0;

            i++;
        }
    }

    public static int Compare(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2, int maxLen)
    {
        for (int i = 0; i < maxLen; i++)
        {
            int c1 = ((uint)i < (uint)s1.Length ? s1[i] : 0);
            int c2 = ((uint)i < (uint)s2.Length ? s2[i] : 0);

            if (c1 != c2)
                return c1 - c2;

            if (c1 == 0)
                return 0;
        }

        return 0;
    }

    public static int CompareCaseInsensitive(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2)
    {
        int i = 0;

        while (true)
        {
            int c1 = ((uint)i < (uint)s1.Length ? ToLowerAsciiInvariant(s1[i]) : 0);
            int c2 = ((uint)i < (uint)s2.Length ? ToLowerAsciiInvariant(s2[i]) : 0);

            if (c1 != c2)
                return c1 - c2;

            if (c1 == 0)
                return 0;

            i++;
        }
    }

    public static int CompareCaseInsensitive(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2, int maxLen)
    {
        for (int i = 0; i < maxLen; i++)
        {
            int c1 = ((uint)i < (uint)s1.Length ? ToLowerAsciiInvariant(s1[i]) : 0);
            int c2 = ((uint)i < (uint)s2.Length ? ToLowerAsciiInvariant(s2[i]) : 0);

            if (c1 != c2)
                return c1 - c2;

            if (c1 == 0)
                return 0;
        }

        return 0;
    }

    private static byte ToLowerAsciiInvariant(byte c)
    {
        if (unchecked((uint)(c - 'A') <= 'Z' - 'A'))
        {
            c = (byte)(c | 0x20);
        }

        return c;
    }

    public static int Find(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle)
    {
        if (needle.Length == 0)
            return 0;

        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            int j;
            for (j = 0; j < needle.Length && haystack[i + j] == needle[j]; j++)
            { }

            if (j == needle.Length)
                return i;
        }

        return -1;
    }

    public static ReadOnlySpan<byte> StringToUtf8(string value)
    {
        return Encoding.UTF8.GetBytes(value).AsSpan();
    }

    public static string Utf8ToString(ReadOnlySpan<byte> value)
    {
        return Encoding.UTF8.GetString(value);
    }

    public static string Utf8ZToString(ReadOnlySpan<byte> value)
    {
        return Utf8ToString(value.Slice(0, GetLength(value)));
    }

    public static bool IsAlpha(byte c)
    {
        return unchecked((c | 0x20u) - (byte)'a' <= 'z' - 'a');
    }

    public static bool IsDigit(byte c)
    {
        return unchecked((uint)(c - (byte)'0') <= 9);
    }

    public static string ToHexString(this Span<byte> bytes) => ToHexString((ReadOnlySpan<byte>)bytes);

    public static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        return HexConverter.ToString(bytes);
    }
}