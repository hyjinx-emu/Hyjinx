using LibHac.Kernel;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using static LibHac.Kernel.InitialProcessBinaryReader;
using static LibHac.Kernel.KipHeader;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Kernel;

public class TypeLayoutTests
{
    [Test]
    public static void IniHeader_Layout()
    {
        var s = new IniHeader();

        Assert.That(Unsafe.SizeOf<IniHeader>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.ProcessCount), Is.EqualTo(0x8));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0xC));
    }

    [Test]
    public static void KipHeader_Layout()
    {
        var s = new KipHeader();

        Assert.That(Unsafe.SizeOf<KipHeader>(), Is.EqualTo(0x100));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Name), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.ProgramId), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.Priority), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.IdealCoreId), Is.EqualTo(0x1D));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x1F));
        Assert.That(GetOffset(in s, in s.TextMemoryOffset), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.TextSize), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.TextFileSize), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.AffinityMask), Is.EqualTo(0x2C));
        Assert.That(GetOffset(in s, in s.RoMemoryOffset), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.RoSize), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.RoFileSize), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.StackSize), Is.EqualTo(0x3C));
        Assert.That(GetOffset(in s, in s.DataMemoryOffset), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.DataSize), Is.EqualTo(0x44));
        Assert.That(GetOffset(in s, in s.DataFileSize), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.BssMemoryOffset), Is.EqualTo(0x50));
        Assert.That(GetOffset(in s, in s.BssSize), Is.EqualTo(0x54));
        Assert.That(GetOffset(in s, in s.BssFileSize), Is.EqualTo(0x58));
        Assert.That(GetOffset(in s, in s.Capabilities), Is.EqualTo(0x80));

        Assert.That(GetOffset(in s, in s.Segments[0]), Is.EqualTo(0x20));
    }

    [Test]
    public static void KipSegmentHeader_Layout()
    {
        var s = new SegmentHeader();

        Assert.That(Unsafe.SizeOf<SegmentHeader>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.MemoryOffset), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x4));
        Assert.That(GetOffset(in s, in s.FileSize), Is.EqualTo(0x8));
    }
}