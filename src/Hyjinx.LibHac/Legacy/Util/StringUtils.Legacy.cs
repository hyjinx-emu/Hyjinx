#if IS_LEGACY_ENABLED

using LibHac.Util.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHac.Util;

partial class StringUtils
{
    /// <summary>
    /// Concatenates 2 byte strings.
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="source"></param>
    /// <returns>The length of the resulting string.</returns>
    /// <remarks>This function appends the source string to the end of the null-terminated destination string.
    /// If the destination buffer is not large enough to contain the resulting string,
    /// bytes from the source string will be appended to the destination string util the buffer is full.
    /// If the length of the final string is the same length of the destination buffer,
    /// no null terminating byte will be written to the end of the string.</remarks>
    public static int Concat(Span<byte> dest, ReadOnlySpan<byte> source)
    {
        return Concat(dest, source, GetLength(dest));
    }

    public static int Concat(Span<byte> dest, ReadOnlySpan<byte> source, int destLength)
    {
        int iDest = destLength;

        for (int i = 0; iDest < dest.Length && i < source.Length && source[i] != 0; i++, iDest++)
        {
            dest[iDest] = source[i];
        }

        if (iDest < dest.Length)
        {
            dest[iDest] = 0;
        }

        return iDest;
    }

    public static bool IsHexDigit(byte c)
    {
        return HexConverter.CharToHexLookup[c] != 0xFF;
    }

    public static string NullTerminatedUtf8ToString(ReadOnlySpan<byte> value)
    {
        int length = GetLength(value);

        return Encoding.UTF8.GetString(value.Slice(0, length));
    }

    public static byte[] ToBytes(this string input)
    {
        return FromHexString(input);
    }

    public static byte[] FromHexString(string input)
    {
        if ((uint)input.Length % 2 != 0)
            throw new FormatException("Hex input must be a multiple of 2.");

        byte[] result = new byte[input.Length >> 1];

        if (!HexConverter.TryDecodeFromUtf16(input, result))
        {
            throw new FormatException("Hex input contains invalid characters.");
        }

        return result;
    }

    public static void FromHexString(ReadOnlySpan<char> chars, Span<byte> outputBytes)
    {
        if ((uint)chars.Length % 2 != 0)
            throw new FormatException("Hex input must be a multiple of 2.");

        uint bytesLength = (uint)chars.Length / 2;

        if ((uint)outputBytes.Length >= bytesLength)
        {
            Span<byte> bytes = outputBytes.Slice(0, (int)bytesLength);

            if (!HexConverter.TryDecodeFromUtf16(chars, bytes))
            {
                throw new FormatException("Hex input contains invalid characters.");
            }
        }

        throw new ArgumentException("Buffer is not large enough to fit the input hex string.", nameof(outputBytes));
    }

    public static string ToHexString(this byte[] bytes) => ToHexString((ReadOnlySpan<byte>)bytes);

    public static bool TryFromHexString(ReadOnlySpan<char> chars, Span<byte> outputBytes)
    {
        if ((uint)chars.Length % 2 != 0)
            return false;

        uint bytesLength = (uint)chars.Length / 2;

        if ((uint)outputBytes.Length >= bytesLength)
        {
            Span<byte> bytes = outputBytes.Slice(0, (int)bytesLength);
            return HexConverter.TryDecodeFromUtf16(chars, bytes);
        }

        return false;
    }
}

#endif