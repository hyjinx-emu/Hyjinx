using LibHac.Util;

namespace LibHac.Tests.Util;

public class IntUtilTests
{
    [TestCase((byte)(0x80), (byte)(0x7F), true)]
    [TestCase((byte)(0x80), (byte)(0x80), false)]
    public void CanAddWithoutOverflow_Byte(byte x, byte y, bool expectedResult)
    {
        Assert.That(IntUtil.CanAddWithoutOverflow(x, y), Is.EqualTo(expectedResult));
        Assert.That(IntUtil.CanAddWithoutOverflow(y, x), Is.EqualTo(expectedResult));
    }

    [TestCase((short)(-0x8000), (short)(-1), false)]
    [TestCase((short)(-0x8000), (short)(0), true)]
    [TestCase((short)(0x4000), (short)(0x3FFF), true)]
    [TestCase((short)(0x4000), (short)(0x4000), false)]
    [TestCase((short)(-0x4000), (short)(-0x4000), true)]
    public void CanAddWithoutOverflow_Short(short x, short y, bool expectedResult)
    {
        Assert.That(IntUtil.CanAddWithoutOverflow(x, y), Is.EqualTo(expectedResult));
        Assert.That(IntUtil.CanAddWithoutOverflow(y, x), Is.EqualTo(expectedResult));
    }

    [TestCase((byte)(0x80), (byte)(0x80), true)]
    [TestCase((byte)(0x80), (byte)(0x81), false)]
    public void CanSubtractWithoutOverflow_Byte(byte x, byte y, bool expectedResult)
    {
        Assert.That(IntUtil.CanSubtractWithoutOverflow(x, y), Is.EqualTo(expectedResult));
    }

    [TestCase((short)(-0x8000), (short)(1), false)]
    [TestCase((short)(-0x8000), (short)(0), true)]
    [TestCase((short)(-0x4000), (short)(0x4000), true)]
    [TestCase((short)(-0x4000), (short)(0x4001), false)]
    [TestCase((short)(0x4000), (short)(-0x4000), false)]
    public void CanSubtractWithoutOverflow_Short(short x, short y, bool expectedResult)
    {
        Assert.That(IntUtil.CanSubtractWithoutOverflow(x, y), Is.EqualTo(expectedResult));
    }

    [TestCase((byte)(0xFF), (byte)(0), true)]
    [TestCase((byte)(0x55), (byte)(3), true)]
    [TestCase((byte)(0x40), (byte)(4), false)]
    public void CanMultiplyWithoutOverflow_Byte(byte x, byte y, bool expectedResult)
    {
        Assert.That(IntUtil.CanMultiplyWithoutOverflow(x, y), Is.EqualTo(expectedResult));
        Assert.That(IntUtil.CanMultiplyWithoutOverflow(y, x), Is.EqualTo(expectedResult));
    }

    [TestCase((short)(0x7FFF), (short)(0), true)]
    [TestCase((short)(+0x1249), (short)(+7), true)]
    [TestCase((short)(-0x1249), (short)(+7), true)]
    [TestCase((short)(-0x1249), (short)(-7), true)]
    [TestCase((short)(+0x1249), (short)(-7), true)]
    [TestCase((short)(+0x2000), (short)(+4), false)]
    [TestCase((short)(-0x2000), (short)(+4), true)]
    [TestCase((short)(-0x2000), (short)(-4), false)]
    [TestCase((short)(+0x2000), (short)(-4), true)]
    [TestCase((short)(-0x2001), (short)(+4), false)]
    [TestCase((short)(+0x2001), (short)(-4), false)]
    [TestCase((short)(-0x8000), (short)(-1), false)]
    [TestCase((short)(-0x7FFF), (short)(-1), true)]
    public void CanMultiplyWithoutOverflow_Short(short x, short y, bool expectedResult)
    {
        Assert.That(IntUtil.CanMultiplyWithoutOverflow(x, y), Is.EqualTo(expectedResult));
        Assert.That(IntUtil.CanMultiplyWithoutOverflow(y, x), Is.EqualTo(expectedResult));
    }

    [TestCase((ulong)(0x5555555555555555), (ulong)(3), true)]
    [TestCase((ulong)(0x4000000000000000), (ulong)(4), false)]
    public void CanMultiplyWithoutOverflow_Ulong(ulong x, ulong y, bool expectedResult)
    {
        Assert.That(IntUtil.CanMultiplyWithoutOverflow(x, y), Is.EqualTo(expectedResult));
        Assert.That(IntUtil.CanMultiplyWithoutOverflow(y, x), Is.EqualTo(expectedResult));
    }

    [TestCase((long)(0), true)]
    [TestCase((long)(-1), false)]
    [TestCase((long)(unchecked((long)0x8111111100000000)), false)]
    [TestCase((long)(long.MinValue), false)]
    [TestCase((long)(long.MaxValue), true)]
    public void IsIntValueRepresentable_LongToUlong(long value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<ulong, long>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((ulong)(0), true)]
    [TestCase((ulong)(5), true)]
    [TestCase((ulong)(long.MaxValue), true)]
    [TestCase((ulong)(long.MaxValue + 1LU), false)]
    [TestCase((ulong)(ulong.MaxValue), false)]
    public void IsIntValueRepresentable_ULongToLong(ulong value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<long, ulong>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((long)(0), true)]
    [TestCase((long)(-1), true)]
    [TestCase((long)(int.MinValue), true)]
    [TestCase((long)(int.MaxValue), true)]
    [TestCase((long)(int.MinValue - 1L), false)]
    [TestCase((long)(int.MaxValue + 1L), false)]
    public void IsIntValueRepresentable_LongToInt(long value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<int, long>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((long)(0), true)]
    [TestCase((long)(-1), true)]
    [TestCase((long)(short.MinValue), true)]
    [TestCase((long)(short.MaxValue), true)]
    [TestCase((long)(short.MinValue - 1L), false)]
    [TestCase((long)(short.MaxValue + 1L), false)]
    public void IsIntValueRepresentable_LongToShort(long value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<short, long>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((long)(0), true)]
    [TestCase((long)(-1), false)]
    [TestCase((long)(uint.MaxValue), true)]
    [TestCase((long)(uint.MaxValue + 1L), false)]
    public void IsIntValueRepresentable_LongToUint(long value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<uint, long>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((long)(0), true)]
    [TestCase((long)(-1), false)]
    [TestCase((long)(ushort.MaxValue), true)]
    [TestCase((long)(ushort.MaxValue + 1L), false)]
    public void IsIntValueRepresentable_LongToUshort(long value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<ushort, long>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((short)(0), true)]
    [TestCase((short)(-1), false)]
    [TestCase((short)(short.MinValue), false)]
    [TestCase((short)(short.MaxValue), true)]
    public void IsIntValueRepresentable_ShortToUshort(short value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<ushort, short>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((uint)(0), true)]
    [TestCase((uint)(0xFFFFF000), false)]
    [TestCase((uint)(0xFFFF7FFF), false)]
    [TestCase((uint)(short.MaxValue), true)]
    [TestCase((uint)(short.MaxValue + 1), false)]
    public void IsIntValueRepresentable_UintToShort(uint value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<short, uint>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((ulong)(0), true)]
    [TestCase((ulong)(uint.MaxValue), true)]
    [TestCase((ulong)uint.MaxValue + 1, false)]
    public void IsIntValueRepresentable_UlongToUint(ulong value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<uint, ulong>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase((uint)(0), true)]
    [TestCase((uint)(uint.MaxValue), true)]
    public void IsIntValueRepresentable_UintToUlong(uint value, bool expectedResult)
    {
        bool actualResult = IntUtil.IsIntValueRepresentable<ulong, uint>(value);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }
}