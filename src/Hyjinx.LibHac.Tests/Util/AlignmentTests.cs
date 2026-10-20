using LibHac.Util;
using NUnit.Framework;

namespace LibHac.Tests.Util;

public class AlignmentTests
{
    [TestCase((byte)(0), (uint)(0x40), (byte)(0))]
    [TestCase((byte)(0x3F), (uint)(0x40), (byte)(0x40))]
    [TestCase((byte)(0x40), (uint)(0x40), (byte)(0x40))]
    [TestCase((byte)(0x41), (uint)(0x40), (byte)(0x80))]
    public void AlignUp_Byte(byte value, uint alignment, byte expectedValue)
    {
        var actualValue = Alignment.AlignUp(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((int)(0), (uint)(0x40), (int)(0))]
    [TestCase((int)(-0x3F), (uint)(0x40), (int)(0))]
    [TestCase((int)(-0x40), (uint)(0x40), (int)(-0x40))]
    [TestCase((int)(-0x41), (uint)(0x40), (int)(-0x40))]
    [TestCase((int)(-0x41), (uint)(0), (int)(0))]
    [TestCase((int)(int.MaxValue), (uint)(0x40), (int)(int.MinValue))]
    public void AlignUp_Int(int value, uint alignment, int expectedValue)
    {
        var actualValue = Alignment.AlignUp(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((ulong)(0), (uint)(0x40), (ulong)(0))]
    [TestCase((ulong)(0x3F), (uint)(0x40), (ulong)(0x40))]
    [TestCase((ulong)(0x40), (uint)(0x40), (ulong)(0x40))]
    [TestCase((ulong)(0x41), (uint)(0x40), (ulong)(0x80))]
    [TestCase((ulong)(0xFFF_FFFF_8000), (uint)(0x10000), (ulong)(0x1000_0000_0000))]
    public void AlignUp_Ulong(ulong value, uint alignment, ulong expectedValue)
    {
        var actualValue = Alignment.AlignUp(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((byte)(0), (uint)(0x40), (byte)(0))]
    [TestCase((byte)(0x3F), (uint)(0x40), (byte)(0))]
    [TestCase((byte)(0x40), (uint)(0x40), (byte)(0x40))]
    [TestCase((byte)(0x41), (uint)(0x40), (byte)(0x40))]
    public void AlignDown_Byte(byte value, uint alignment, byte expectedValue)
    {
        var actualValue = Alignment.AlignDown(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((long)(0), (uint)(0x40), (long)(0))]
    [TestCase((long)(0x3F), (uint)(0x40), (long)(0))]
    [TestCase((long)(0x40), (uint)(0x40), (long)(0x40))]
    [TestCase((long)(0x41), (uint)(0x40), (long)(0x40))]
    public void AlignDown_Long(long value, uint alignment, long expectedValue)
    {
        var actualValue = Alignment.AlignDown(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((byte)(0), (uint)(0x40), true)]
    [TestCase((byte)(0x3F), (uint)(0x40), false)]
    [TestCase((byte)(0x40), (uint)(0x40), true)]
    [TestCase((byte)(0x41), (uint)(0x40), false)]
    public void IsAligned_Byte(byte value, uint alignment, bool expectedValue)
    {
        var actualValue = Alignment.IsAligned(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((long)(0), (uint)(0x40), true)]
    [TestCase((long)(0x3F), (uint)(0x40), false)]
    [TestCase((long)(0x40), (uint)(0x40), true)]
    [TestCase((long)(0x41), (uint)(0x40), false)]
    [TestCase((long)(0xFFF_FFFF_8000), (uint)(0x400), true)]
    public void IsAligned_Long(long value, uint alignment, bool expectedValue)
    {
        var actualValue = Alignment.IsAligned(value, alignment);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
    [TestCase((uint)(0), (long)(0))]
    [TestCase((uint)(0x3F), (long)(1))]
    [TestCase((uint)(0x40), (long)(0x40))]
    [TestCase((uint)(0x41), (long)(1))]
    [TestCase((uint)(0x42), (long)(2))]
    [TestCase((uint)(0xFF900000), (long)(0x100000))]
    public void GetAlignment_Uint(uint value, long expectedValue)
    {
        var actualValue = Alignment.GetAlignment(value);
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }
}