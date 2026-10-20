// ReSharper disable InconsistentNaming

using LibHac.Fs;
using System.Runtime.CompilerServices;

namespace LibHac.Tests.Fs;

public class LazyLoadTypeTests
{
    [Test]
    public static void UnpreparedRangeInfoSizeIs0x40()
    {
        Assert.That(Unsafe.SizeOf<UnpreparedRangeInfo>(), Is.EqualTo(0x40));
    }

    [Test]
    public static void UnpreparedFileInformationSizeIs0x301()
    {
        Assert.That(Unsafe.SizeOf<UnpreparedFileInformation>(), Is.EqualTo(0x301));
    }

    [Test]
    public static void LazyLoadArgumentsSizeIs0x40()
    {
        Assert.That(Unsafe.SizeOf<LazyLoadArguments>(), Is.EqualTo(0x40));
    }
}