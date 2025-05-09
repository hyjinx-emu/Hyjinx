using System;

namespace LibHac.Crypto;

internal interface ICipher
{
    int Transform(ReadOnlySpan<byte> input, Span<byte> output);
}
