using System;

namespace LibHac.Crypto;

internal interface IHash
{
    void Initialize();
    void Update(ReadOnlySpan<byte> data);
    void GetHash(Span<byte> hashBuffer);
}