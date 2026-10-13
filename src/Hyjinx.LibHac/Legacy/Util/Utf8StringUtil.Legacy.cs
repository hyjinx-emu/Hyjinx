#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Diag;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Util;

partial class Utf8StringUtil
{
    public static int CopyUtf8String(Span<byte> output, ReadOnlySpan<byte> input, int maxCount)
    {
        Assert.SdkRequiresGreater(output.Length, 0);
        Assert.SdkRequiresGreater(input.Length, 0);
        Assert.SdkRequiresGreater(maxCount, 0);

        ReadOnlySpan<byte> currentInput = input;
        int remainingCount = maxCount;

        while (remainingCount > 0 && currentInput.Length != 0)
        {
            // Verify the current code point
            int codePointLength = GetCodePointByteLength(currentInput[0]);
            if (codePointLength > currentInput.Length)
                break;

            if (!VerifyCode(currentInput.Slice(0, codePointLength)))
                break;

            // Ensure the output is large enough to hold the additional code point
            int currentOutputLength =
                Unsafe.ByteOffset(ref MemoryMarshal.GetReference(input), ref MemoryMarshal.GetReference(currentInput))
                    .ToInt32() + codePointLength;

            if (currentOutputLength + 1 > output.Length)
                break;

            // Advance to the next code point
            currentInput = currentInput.Slice(codePointLength);
            remainingCount--;
        }

        // Copy the valid UTF-8 to the output buffer
        int byteLength = Unsafe
            .ByteOffset(ref MemoryMarshal.GetReference(input), ref MemoryMarshal.GetReference(currentInput)).ToInt32();

        Assert.SdkAssert(byteLength + 1 <= output.Length);

        if (byteLength != 0)
            input.Slice(0, byteLength).CopyTo(output);

        output[byteLength] = 0;
        return byteLength;
    }
}

#endif