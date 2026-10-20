using LibHac.Bcat;
using LibHac.Bcat.Impl.Service.Core;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Bcat;

public class TypeLayoutTests
{
    [Test]
    public static void Digest_Layout()
    {
        var s = new Digest();

        Assert.That(Unsafe.SizeOf<Digest>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x0));
    }

    [Test]
    public static void DirectoryName_Layout()
    {
        var s = new DirectoryName();

        Assert.That(Unsafe.SizeOf<DirectoryName>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x0));
    }

    [Test]
    public static void FileName_Layout()
    {
        var s = new FileName();

        Assert.That(Unsafe.SizeOf<FileName>(), Is.EqualTo(0x20));

        Assert.That(GetOffset(in s, in s.Value), Is.EqualTo(0x0));
    }

    [Test]
    public static void DeliveryCacheDirectoryEntry_Layout()
    {
        var s = new DeliveryCacheDirectoryEntry();

        Assert.That(Unsafe.SizeOf<DeliveryCacheDirectoryEntry>(), Is.EqualTo(0x38));

        Assert.That(GetOffset(in s, in s.Name), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.Digest), Is.EqualTo(0x28));
    }

    [Test]
    public static void DeliveryCacheDirectoryMetaEntry_Layout()
    {
        var s = new DeliveryCacheDirectoryMetaEntry();

        Assert.That(Unsafe.SizeOf<DeliveryCacheDirectoryMetaEntry>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.Name), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Digest), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x30));
    }

    [Test]
    public static void DeliveryCacheFileMetaEntry_Layout()
    {
        var s = new DeliveryCacheFileMetaEntry();

        Assert.That(Unsafe.SizeOf<DeliveryCacheFileMetaEntry>(), Is.EqualTo(0x80));

        Assert.That(GetOffset(in s, in s.Name), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Id), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.Digest), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x40));
    }
}