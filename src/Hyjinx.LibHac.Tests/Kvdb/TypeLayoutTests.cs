using LibHac.Kvdb;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Kvdb;

public class TypeLayoutTests
{
    [Test]
    public static void KeyValueArchiveHeader_Layout()
    {
        var s = new KeyValueArchiveHeader();

        Assert.That(Unsafe.SizeOf<KeyValueArchiveHeader>(), Is.EqualTo(0xC));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.EntryCount), Is.EqualTo(0x8));
    }

    [Test]
    public static void KeyValueArchiveEntryHeader_Layout()
    {
        var s = new KeyValueArchiveEntryHeader();

        Assert.That(Unsafe.SizeOf<KeyValueArchiveEntryHeader>(), Is.EqualTo(0xC));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.KeySize), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.ValueSize), Is.EqualTo(0x8));
    }
}