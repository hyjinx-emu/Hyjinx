using LibHac.Common;
using Xunit;

namespace LibHac.Tests.Common;

public class RentedArray2ExtensionsTests
{
    [Fact]
    public void ReturnsTheMemoryStream()
    {
        var expected = 0x10;

        using var buffer = new RentedArray2<byte>(expected);
        using var ms = buffer.AsMemoryStream();

        Assert.Equal(expected, ms.Length);
    }
}