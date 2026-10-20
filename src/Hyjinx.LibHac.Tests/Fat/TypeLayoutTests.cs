using LibHac.Fat;
using System.Runtime.CompilerServices;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Fat;

public class TypeLayoutTests
{
    [Test]
    public static void FatError_Layout()
    {
        var s = new FatError();

        Assert.That(Unsafe.SizeOf<FatError>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Error), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.ExtraError), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.DriveId), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.ErrorName), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x1C));
    }
}