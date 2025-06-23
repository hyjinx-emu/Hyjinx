using LibHac.Common;
using Xunit;

namespace LibHac.Tests.Common;

public class RentedArray2Tests
{
    [Fact]
    public void AlwaysReturnsTheRightSize()
    {
        using var target = new RentedArray2<byte>(6080);

        var array = target.ToArray();
        Assert.Equal(8192, array.Length);
        
        Assert.Equal(6080, target.Span.Length);
        Assert.Equal(6080, target.Memory.Length);
    }
}