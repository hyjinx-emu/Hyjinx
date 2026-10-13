using LibHac.Common;
using LibHac.Diag;
using System;

namespace LibHac.Util;

/// <summary>
/// Contains functions for verifying and copying UTF-8 strings.
/// </summary>
/// <remarks>Based on nnSdk 13.4.0</remarks>
public static partial class Utf8StringUtil
{
    private static ReadOnlySpan<byte> CodePointByteLengthTable => new byte[]
    {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
        4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    public static bool VerifyUtf8String(U8Span str)
    {
        return GetCodePointCountOfUtf8String(str) != -1;
    }

    public static int GetCodePointCountOfUtf8String(U8Span str)
    {
        Assert.SdkRequiresGreater(str.Length, 0);

        ReadOnlySpan<byte> currentStr = str.Value;
        int codePointCount = 0;

        while (currentStr.Length != 0)
        {
            int codePointByteLength = GetCodePointByteLength(currentStr[0]);

            if (codePointByteLength > currentStr.Length)
                return -1;

            if (!VerifyCode(currentStr.Slice(0, codePointByteLength)))
                return -1;

            currentStr = currentStr.Slice(codePointByteLength);

            codePointCount++;
        }

        return codePointCount;
    }

    private static int GetCodePointByteLength(byte head)
    {
        return CodePointByteLengthTable[head];
    }

    private static bool IsValidTail(byte tail)
    {
        return (tail & 0xC0) == 0x80;
    }

    private static bool VerifyCode(ReadOnlySpan<byte> str)
    {
        if (str.Length == 1)
            return true;

        switch (str.Length)
        {
            case 2:
                if (!IsValidTail(str[1]))
                    return false;

                break;
            case 3:
                if (str[0] == 0xE0 && (str[1] & 0x20) == 0)
                    return false;

                if (str[0] == 0xED && (str[1] & 0x20) != 0)
                    return false;

                if (!IsValidTail(str[1]) || !IsValidTail(str[2]))
                    return false;

                break;
            case 4:
                if (str[0] == 0xF0 && (str[1] & 0x30) == 0)
                    return false;

                if (str[0] == 0xFD && (str[1] & 0x30) != 0)
                    return false;

                if (!IsValidTail(str[1]) || !IsValidTail(str[2]) || !IsValidTail(str[3]))
                    return false;

                break;
            default:
                return false;
        }

        return true;
    }
}