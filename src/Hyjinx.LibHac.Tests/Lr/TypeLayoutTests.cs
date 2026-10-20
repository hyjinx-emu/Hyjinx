using System.Runtime.CompilerServices;
using static LibHac.Tests.Common.Layout;
using Path = LibHac.Lr.Path;

namespace LibHac.Tests.Lr;

public class TypeLayoutTests
{
    [Test]
    public static void IniHeader_Layout()
    {
        var s = new Path();

        Assert.That(Unsafe.SizeOf<Path>(), Is.EqualTo(0x300));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x0));
    }
}