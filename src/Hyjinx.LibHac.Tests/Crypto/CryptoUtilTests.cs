using LibHac.Common;
using LibHac.Crypto;

namespace LibHac.Tests.Crypto;

public class CryptoUtilTests
{
    [Test]
    public void ReturnsValidWhenAllZeroesExpected()
    {
        Span<byte> expected = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        Span<byte> input = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0];

        var result = CryptoUtil.CheckSha256Hash(input, expected);

        Assert.That(result, Is.EqualTo(Validity.Valid));
    }

    [Test]
    public void ReturnsValidWhenHashesMatch()
    {
        Span<byte> input = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0];
        Span<byte> expected = [154, 137, 198, 140, 76, 94, 40, 184, 196, 165, 86, 118, 115, 212, 98, 255, 245, 21, 219, 70, 17, 111, 153, 0, 98, 77, 9, 196, 116, 245, 147, 251];

        var result = CryptoUtil.CheckSha256Hash(input, expected);

        Assert.That(result, Is.EqualTo(Validity.Valid));
    }

    [Test]
    public void ReturnsInvalidWhenHashesDoNotMatch()
    {
        Span<byte> input = [1, 2, 3, 4, 5, 6, 7, 8, 9, 0];
        Span<byte> expected = [1, 2, 3, 4, 5];

        var result = CryptoUtil.CheckSha256Hash(input, expected);

        Assert.That(result, Is.EqualTo(Validity.Invalid));
    }
}