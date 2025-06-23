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
    
    [Fact]
    public void HandlesLargeRentals()
    {
        using var target = new RentedArray2<byte>(8192);

        var array = target.ToArray();
        Assert.Equal(8192, array.Length);
        
        Assert.Equal(8192, target.Span.Length);
        Assert.Equal(8192, target.Memory.Length);
    }
    
    [Fact]
    public void HandlesLargeRentalsWithClearEnabled()
    {
        using var target = new RentedArray2<byte>(8192, true);

        var array = target.ToArray();
        Assert.Equal(8192, array.Length);
        
        Assert.Equal(8192, target.Span.Length);
        Assert.Equal(8192, target.Memory.Length);
    }
    
    [Fact]
    public void HandlesSmallRentals()
    {
        using var target = new RentedArray2<byte>(16);

        var array = target.ToArray();
        Assert.Equal(16, array.Length);
        
        Assert.Equal(16, target.Span.Length);
        Assert.Equal(16, target.Memory.Length);
    }
    
    [Fact]
    public void HandlesSmallRentalsWithClearEnabled()
    {
        using var target = new RentedArray2<byte>(16, true);

        var array = target.ToArray();
        Assert.Equal(16, array.Length);
        
        Assert.Equal(16, target.Span.Length);
        Assert.Equal(16, target.Memory.Length);
    }
}