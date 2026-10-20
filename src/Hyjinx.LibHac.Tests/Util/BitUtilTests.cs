using LibHac.Util;
using NUnit.Framework;

namespace LibHac.Tests.Util;

public class BitUtilTests
{
    [TestCase((byte)(0x80), (int)(0))]
    [TestCase((byte)(0x00), (int)(8))]
    public void CountLeadingZeros_Byte(byte value, int expectedValue)
    {
        Assert.That(BitUtil.CountLeadingZeros(value), Is.EqualTo(expectedValue));
    }
    [TestCase((short)(0x0080), (int)(8))]
    [TestCase((short)(0x0000), (int)(16))]
    public void CountLeadingZeros_Short(short value, int expectedValue)
    {
        Assert.That(BitUtil.CountLeadingZeros(value), Is.EqualTo(expectedValue));
    }
    [TestCase((short)(0b0000_1000_0110_0000), (short)(0b0000_1000_0100_0000))]
    [TestCase((short)(0b0000_1000_0110_1111), (short)(0b0000_1000_0110_1110))]
    [TestCase((short)(unchecked((short)0b1000_0000_0000_0000)), (short)(0b0000_0000_0000_0000))]
    [TestCase((short)(0), (short)(0))]
    public void ResetLeastSignificantOneBit_Short(short value, short expectedValue)
    {
        Assert.That(BitUtil.ResetLeastSignificantOneBit(value), Is.EqualTo(expectedValue));
    }
    [TestCase((uint)(0b0101_0000_0000_0000_0000), (uint)(0b0100_0000_0000_0000_0000))]
    [TestCase((uint)(0b1111_0000_1000_0110_1111), (uint)(0b1111_0000_1000_0110_1110))]
    [TestCase((uint)(0b0100_1000_0000_0000_0000), (uint)(0b0100_0000_0000_0000_0000))]
    [TestCase((uint)(0), (uint)(0))]
    public void ResetLeastSignificantOneBit_Uint(uint value, uint expectedValue)
    {
        Assert.That(BitUtil.ResetLeastSignificantOneBit(value), Is.EqualTo(expectedValue));
    }
    [TestCase((short)(0x80), true)]
    [TestCase((short)(0x81), false)]
    [TestCase((short)(0), false)]
    [TestCase((short)(short.MinValue), false)]
    public void IsPowerOfTwo_Short(short value, bool expectedValue)
    {
        Assert.That(BitUtil.IsPowerOfTwo(value), Is.EqualTo(expectedValue));
    }
    [TestCase((ulong)(0x0000100000000000), true)]
    [TestCase((ulong)(0x0000100000004000), false)]
    [TestCase((ulong)(0), false)]
    public void IsPowerOfTwo_Ulong(ulong value, bool expectedValue)
    {
        Assert.That(BitUtil.IsPowerOfTwo(value), Is.EqualTo(expectedValue));
    }
    [TestCase((int)(-55), (int)(-2), (int)(29))]
    [TestCase((int)(-55), (int)(2), (int)(-27))]
    [TestCase((int)(0), (int)(26), (int)(0))]
    [TestCase((int)(-55), (int)(-26), (int)(3))]
    [TestCase((int)(55), (int)(-26), (int)(-1))]
    [TestCase((int)(int.MinValue), (int)(-26), (int)(-82595523))]
    public void DivideUp_Int(int value, int divisor, int expectedValue)
    {
        Assert.That(BitUtil.DivideUp(value, divisor), Is.EqualTo(expectedValue));
    }
    [TestCase((uint)(55), (uint)(2), (uint)(28))]
    [TestCase((uint)(0), (uint)(26), (uint)(0))]
    [TestCase((uint)(1127), (uint)(24), (uint)(47))]
    [TestCase((uint)(1128), (uint)(24), (uint)(47))]
    [TestCase((uint)(1129), (uint)(24), (uint)(48))]
    [TestCase((uint)(567), (uint)(987), (uint)(1))]
    [TestCase((uint)(int.MaxValue), (uint)(26), (uint)(82595525))]
    public void DivideUp_Uint(uint value, uint divisor, uint expectedValue)
    {
        Assert.That(BitUtil.DivideUp(value, divisor), Is.EqualTo(expectedValue));
    }
}