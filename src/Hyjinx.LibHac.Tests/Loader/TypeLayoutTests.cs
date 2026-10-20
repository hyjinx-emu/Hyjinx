using LibHac.Loader;
using System.Runtime.CompilerServices;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Loader;

public class TypeLayoutTests
{
    [Test]
    public static void NsoHeader_Layout()
    {
        var s = new NsoHeader();

        Assert.That(Unsafe.SizeOf<NsoHeader>(), Is.EqualTo(0x100));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.Reserved08), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x0C));

        Assert.That(GetOffset(in s, in s.TextFileOffset), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.TextMemoryOffset), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.TextSize), Is.EqualTo(0x18));

        Assert.That(GetOffset(in s, in s.ModuleNameOffset), Is.EqualTo(0x1C));

        Assert.That(GetOffset(in s, in s.RoFileOffset), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.RoMemoryOffset), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.RoSize), Is.EqualTo(0x28));

        Assert.That(GetOffset(in s, in s.ModuleNameSize), Is.EqualTo(0x2C));

        Assert.That(GetOffset(in s, in s.DataFileOffset), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.DataMemoryOffset), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.DataSize), Is.EqualTo(0x38));

        Assert.That(GetOffset(in s, in s.BssSize), Is.EqualTo(0x3C));

        Assert.That(GetOffset(in s, in s.ModuleId), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.TextFileSize), Is.EqualTo(0x60));
        Assert.That(GetOffset(in s, in s.RoFileSize), Is.EqualTo(0x64));
        Assert.That(GetOffset(in s, in s.DataFileSize), Is.EqualTo(0x68));

        Assert.That(GetOffset(in s, in s.Reserved6C), Is.EqualTo(0x6C));

        Assert.That(GetOffset(in s, in s.ApiInfoOffset), Is.EqualTo(0x88));
        Assert.That(GetOffset(in s, in s.ApiInfoSize), Is.EqualTo(0x8C));
        Assert.That(GetOffset(in s, in s.DynStrOffset), Is.EqualTo(0x90));
        Assert.That(GetOffset(in s, in s.DynStrSize), Is.EqualTo(0x94));
        Assert.That(GetOffset(in s, in s.DynSymOffset), Is.EqualTo(0x98));
        Assert.That(GetOffset(in s, in s.DynSymSize), Is.EqualTo(0x9C));

        Assert.That(GetOffset(in s, in s.TextHash), Is.EqualTo(0xA0));
        Assert.That(GetOffset(in s, in s.RoHash), Is.EqualTo(0xC0));
        Assert.That(GetOffset(in s, in s.DataHash), Is.EqualTo(0xE0));
    }

    [Test]
    public static void Meta_layout()
    {
        var s = new Meta();

        Assert.That(Unsafe.SizeOf<Meta>(), Is.EqualTo(0x80));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.SignatureKeyGeneration), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.Reserved08), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.Reserved0D), Is.EqualTo(0x0D));
        Assert.That(GetOffset(in s, in s.MainThreadPriority), Is.EqualTo(0x0E));
        Assert.That(GetOffset(in s, in s.DefaultCpuId), Is.EqualTo(0x0F));
        Assert.That(GetOffset(in s, in s.Reserved10), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.SystemResourceSize), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.MainThreadStackSize), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.ProgramName), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.ProductCode), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.Reserved40), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.AciOffset), Is.EqualTo(0x70));
        Assert.That(GetOffset(in s, in s.AciSize), Is.EqualTo(0x74));
        Assert.That(GetOffset(in s, in s.AcidOffset), Is.EqualTo(0x78));
        Assert.That(GetOffset(in s, in s.AcidSize), Is.EqualTo(0x7C));
    }

    [Test]
    public static void AciHeader_Layout()
    {
        var s = new AciHeader();

        Assert.That(Unsafe.SizeOf<AciHeader>(), Is.EqualTo(0x40));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Reserved04), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.ProgramId), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.Reserved18), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.FsAccessControlOffset), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.FsAccessControlSize), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.ServiceAccessControlOffset), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.ServiceAccessControlSize), Is.EqualTo(0x2C));
        Assert.That(GetOffset(in s, in s.KernelCapabilityOffset), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.KernelCapabilitySize), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.Reserved38), Is.EqualTo(0x38));
    }

    [Test]
    public static void AcidHeaderData_Layout()
    {
        var s = new AcidHeaderData();

        Assert.That(Unsafe.SizeOf<AcidHeaderData>(), Is.EqualTo(0x240));

        Assert.That(GetOffset(in s, in s.Signature), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Modulus), Is.EqualTo(0x100));
        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x200));
        Assert.That(GetOffset(in s, in s.Size), Is.EqualTo(0x204));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x208));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x20C));
        Assert.That(GetOffset(in s, in s.ProgramIdMin), Is.EqualTo(0x210));
        Assert.That(GetOffset(in s, in s.ProgramIdMax), Is.EqualTo(0x218));
        Assert.That(GetOffset(in s, in s.FsAccessControlOffset), Is.EqualTo(0x220));
        Assert.That(GetOffset(in s, in s.FsAccessControlSize), Is.EqualTo(0x224));
        Assert.That(GetOffset(in s, in s.ServiceAccessControlOffset), Is.EqualTo(0x228));
        Assert.That(GetOffset(in s, in s.ServiceAccessControlSize), Is.EqualTo(0x22C));
        Assert.That(GetOffset(in s, in s.KernelCapabilityOffset), Is.EqualTo(0x230));
        Assert.That(GetOffset(in s, in s.KernelCapabilitySize), Is.EqualTo(0x234));
        Assert.That(GetOffset(in s, in s.Reserved238), Is.EqualTo(0x238));
    }
}