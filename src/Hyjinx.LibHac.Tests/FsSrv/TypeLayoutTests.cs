using LibHac.FsSrv;
using LibHac.FsSrv.Impl;
using LibHac.FsSrv.Sf;
using LibHac.FsSrv.Storage;
using System.Runtime.CompilerServices;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.FsSrv;

public class TypeLayoutTests
{
    [Test]
    public static void AccessControlDescriptor_Layout()
    {
        var s = new AccessControlDescriptor();

        Assert.That(Unsafe.SizeOf<AccessControlDescriptor>(), Is.EqualTo(0x2C));

        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.ContentOwnerIdCount), Is.EqualTo(0x01));
        Assert.That(GetOffset(in s, in s.SaveDataOwnerIdCount), Is.EqualTo(0x02));
        Assert.That(GetOffset(in s, in s.AccessFlags), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.ContentOwnerIdMin), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.ContentOwnerIdMax), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.SaveDataOwnerIdMin), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.SaveDataOwnerIdMax), Is.EqualTo(0x24));
    }

    [Test]
    public static void AccessControlDataHeader_Layout()
    {
        var s = new AccessControlDataHeader();

        Assert.That(Unsafe.SizeOf<AccessControlDataHeader>(), Is.EqualTo(0x1C));

        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.AccessFlags), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.ContentOwnerInfoOffset), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.ContentOwnerInfoSize), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.SaveDataOwnerInfoOffset), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.SaveDataOwnerInfoSize), Is.EqualTo(0x18));
    }

    [Test]
    public static void Accessibility_Layout()
    {
        Assert.That(Unsafe.SizeOf<Accessibility>(), Is.EqualTo(1));
    }

    [Test]
    public static void FspPath_Layout()
    {
        var s = new FspPath();

        Assert.That(Unsafe.SizeOf<FspPath>(), Is.EqualTo(0x301));

        Assert.That(GetOffset(in s, in s.Str[0]), Is.EqualTo(0x00));
    }

    //[Test]
    //public static void Path_Layout()
    //{
    //    var s = new Path();

    //    Assert.That(Unsafe.SizeOf<Path>(), Is.EqualTo(0x301));

    //    Assert.That(GetOffset(in s, in s.Str[0]), Is.EqualTo(0x00));
    //}

    [Test]
    public static void StorageDeviceHandle_Layout()
    {
        var s = new StorageDeviceHandle();

        Assert.That(Unsafe.SizeOf<StorageDeviceHandle>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.PortId), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x5));
    }

    [Test]
    public static void SaveDataIndexerValue_Layout()
    {
        var s = new SaveDataIndexerValue();

        Assert.That(Unsafe.SizeOf<SaveDataIndexerValue>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.SaveDataId), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Field10), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.SpaceId), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.State), Is.EqualTo(0x19));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x1A));
    }
}