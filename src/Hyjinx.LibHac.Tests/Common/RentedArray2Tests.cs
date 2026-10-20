using LibHac.Common;
using NUnit.Framework;

namespace LibHac.Tests.Common;

public class RentedArray2Tests
{
    [Test]
    public void AlwaysReturnsTheRightSize()
    {
        using var target = new RentedArray2<byte>(6080);

        var array = target.ToArray();
        Assert.That(array.Length, Is.EqualTo(6080));

        Assert.That(target.Span.Length, Is.EqualTo(6080));
        Assert.That(target.Memory.Length, Is.EqualTo(6080));
    }

    [Test]
    public void AlwaysReturnsTheRightSize2()
    {
        var expected = 516;
        using var target = new RentedArray2<byte>(expected);

        var array = target.ToArray();
        Assert.That(array.Length, Is.EqualTo(expected));

        Assert.That(target.Span.Length, Is.EqualTo(expected));
        Assert.That(target.Memory.Length, Is.EqualTo(expected));
    }

    [Test]
    public void HandlesLargeRentals()
    {
        using var target = new RentedArray2<byte>(8192);

        var array = target.ToArray();
        Assert.That(array.Length, Is.EqualTo(8192));

        Assert.That(target.Span.Length, Is.EqualTo(8192));
        Assert.That(target.Memory.Length, Is.EqualTo(8192));
    }

    [Test]
    public void HandlesLargeRentalsWithClearEnabled()
    {
        using var target = new RentedArray2<byte>(8192, true);

        var array = target.ToArray();
        Assert.That(array.Length, Is.EqualTo(8192));

        Assert.That(target.Span.Length, Is.EqualTo(8192));
        Assert.That(target.Memory.Length, Is.EqualTo(8192));
    }

    [Test]
    public void HandlesSmallRentals()
    {
        using var target = new RentedArray2<byte>(16);

        var array = target.ToArray();
        Assert.That(array.Length, Is.EqualTo(16));

        Assert.That(target.Span.Length, Is.EqualTo(16));
        Assert.That(target.Memory.Length, Is.EqualTo(16));
    }

    [Test]
    public void HandlesSmallRentalsWithClearEnabled()
    {
        using var target = new RentedArray2<byte>(16, true);

        var array = target.ToArray();
        Assert.That(array.Length, Is.EqualTo(16));

        Assert.That(target.Span.Length, Is.EqualTo(16));
        Assert.That(target.Memory.Length, Is.EqualTo(16));
    }
}