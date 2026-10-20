using LibHac.Arp;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Arp;

public class TypeLayoutTests
{
    [Test]
    public static void ApplicationLaunchProperty_Layout()
    {
        var s = new ApplicationLaunchProperty();

        Assert.That(Unsafe.SizeOf<ApplicationLaunchProperty>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.ApplicationId), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x8));
        Assert.That(GetOffset(in s, in s.StorageId), Is.EqualTo(0xC));
        Assert.That(GetOffset(in s, in s.PatchStorageId), Is.EqualTo(0xD));
        Assert.That(GetOffset(in s, in s.ApplicationKind), Is.EqualTo(0xE));
    }
}