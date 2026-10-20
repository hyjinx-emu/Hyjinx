// ReSharper disable InconsistentNaming
using LibHac.Spl;
using System.Runtime.CompilerServices;

namespace LibHac.Tests.Spl;

public class TypeSizeTests
{
    [Test]
    public static void AccessKeySizeIs0x10()
    {
        Assert.That(Unsafe.SizeOf<AccessKey>(), Is.EqualTo(0x10));
    }
}