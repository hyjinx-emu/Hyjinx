using LibHac.Common;

namespace LibHac.Tests.Common;

public class RentedArray2ExtensionsTests
{
    [Test]
    public void ReturnsTheMemoryStream()
    {
        var expected = 0x10;

        using var buffer = new RentedArray2<byte>(expected);
        using var ms = buffer.AsMemoryStream();

        Assert.That(ms.Length, Is.EqualTo(expected));
    }
}