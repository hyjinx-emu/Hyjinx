using System;
using LibHac.Crypto;
using Xunit;

namespace LibHac.Tests.CryptoTests;

internal static partial class Common
{
    internal static void HashTestCore(ReadOnlySpan<byte> message, byte[] expectedDigest, IHash hash)
    {
        byte[] digestBuffer = new byte[Sha256.DigestSize];

        hash.Initialize();
        hash.Update(message);
        hash.GetHash(digestBuffer);

        Assert.Equal(expectedDigest, digestBuffer);
    }
}
