using System;
using LibHac.Common;

namespace LibHac.Crypto;

internal interface ICipher
{
    int Transform(ReadOnlySpan<byte> input, Span<byte> output);
}

internal interface ICipherWithIv : ICipher
{
    ref Buffer16 Iv { get; }
}
