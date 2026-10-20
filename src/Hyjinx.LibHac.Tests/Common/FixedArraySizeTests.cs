using LibHac.Common.FixedArrays;
using System.Runtime.CompilerServices;

namespace LibHac.Tests.Common;

public class FixedArraySizeTests
{
    [Test] public static void Array1SpanSizeIsCorrect() => Assert.That(new Array1<byte>().Items.Length, Is.EqualTo(1));
    [Test] public static void Array1ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array1<byte>>(), Is.EqualTo(sizeof(byte) * 1));
    [Test] public static void Array1LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array1<long>>(), Is.EqualTo(sizeof(long) * 1));

    [Test] public static void Array3SpanSizeIsCorrect() => Assert.That(new Array3<byte>().Items.Length, Is.EqualTo(3));
    [Test] public static void Array3ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array3<byte>>(), Is.EqualTo(sizeof(byte) * 3));
    [Test] public static void Array3LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array3<long>>(), Is.EqualTo(sizeof(long) * 3));

    [Test] public static void Array8SpanSizeIsCorrect() => Assert.That(new Array8<byte>().Items.Length, Is.EqualTo(8));
    [Test] public static void Array8ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array8<byte>>(), Is.EqualTo(sizeof(byte) * 8));
    [Test] public static void Array8LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array8<long>>(), Is.EqualTo(sizeof(long) * 8));

    [Test] public static void Array12SpanSizeIsCorrect() => Assert.That(new Array12<byte>().Items.Length, Is.EqualTo(12));
    [Test] public static void Array12ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array12<byte>>(), Is.EqualTo(sizeof(byte) * 12));
    [Test] public static void Array12LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array12<long>>(), Is.EqualTo(sizeof(long) * 12));

    [Test] public static void Array32SpanSizeIsCorrect() => Assert.That(new Array32<byte>().Items.Length, Is.EqualTo(32));
    [Test] public static void Array32ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array32<byte>>(), Is.EqualTo(sizeof(byte) * 32));
    [Test] public static void Array32LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array32<long>>(), Is.EqualTo(sizeof(long) * 32));

    [Test] public static void Array64SpanSizeIsCorrect() => Assert.That(new Array64<byte>().Items.Length, Is.EqualTo(64));
    [Test] public static void Array64ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array64<byte>>(), Is.EqualTo(sizeof(byte) * 64));
    [Test] public static void Array64LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array64<long>>(), Is.EqualTo(sizeof(long) * 64));

    [Test] public static void Array128SpanSizeIsCorrect() => Assert.That(new Array128<byte>().Items.Length, Is.EqualTo(128));
    [Test] public static void Array128ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array128<byte>>(), Is.EqualTo(sizeof(byte) * 128));
    [Test] public static void Array128LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array128<long>>(), Is.EqualTo(sizeof(long) * 128));

    [Test] public static void Array256SpanSizeIsCorrect() => Assert.That(new Array256<byte>().Items.Length, Is.EqualTo(256));
    [Test] public static void Array256ByteSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array256<byte>>(), Is.EqualTo(sizeof(byte) * 256));
    [Test] public static void Array256LongSizeIsCorrect() => Assert.That(Unsafe.SizeOf<Array256<long>>(), Is.EqualTo(sizeof(long) * 256));
}