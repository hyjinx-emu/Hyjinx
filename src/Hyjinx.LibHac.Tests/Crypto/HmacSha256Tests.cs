using LibHac.Crypto;
using Xunit;

namespace LibHac.Tests.Crypto;

public class HmacSha256Tests
{
    [Fact]
    public void CalculatesTheHash()
    {
        var hash = new byte[HmacSha256.HashSize];
        HmacSha256.GenerateHmacSha256(hash, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);

        Assert.Equal(
        [
            192, 138, 176, 27, 137, 124, 49, 112, 1, 76, 220, 66, 160, 209, 52, 72, 38, 30, 142, 136, 247, 81, 169,
            253, 143, 199, 72, 25, 155, 223, 205, 65
        ], hash);
    }
}