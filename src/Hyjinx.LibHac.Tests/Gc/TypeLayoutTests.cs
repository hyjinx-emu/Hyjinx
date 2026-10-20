using LibHac.Gc;
using LibHac.Gc.Impl;
using System.Runtime.CompilerServices;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Gc;

public class TypeLayoutTests
{
    [Test]
    public static void GameCardStatus_Layout()
    {
        var s = new GameCardStatus();

        Assert.That(Unsafe.SizeOf<GameCardStatus>(), Is.EqualTo(0x70));

        Assert.That(GetOffset(in s, in s.PartitionFsHeaderHash), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.PackageId), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.CardSize), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.PartitionFsHeaderAddress), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.PartitionFsHeaderSize), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.NormalAreaSize), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.SecureAreaSize), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.CupVersion), Is.EqualTo(0x50));
        Assert.That(GetOffset(in s, in s.CupId), Is.EqualTo(0x58));
        Assert.That(GetOffset(in s, in s.CompatibilityType), Is.EqualTo(0x60));
        Assert.That(GetOffset(in s, in s.Reserved1), Is.EqualTo(0x61));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x64));
        Assert.That(GetOffset(in s, in s.Reserved2), Is.EqualTo(0x65));
    }

    [Test]
    public static void RmaInformation_Layout()
    {
        var s = new RmaInformation();

        Assert.That(Unsafe.SizeOf<RmaInformation>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.Data), Is.EqualTo(0x00));
    }

    [Test]
    public static void GameCardIdSet_Layout()
    {
        var s = new GameCardIdSet();

        Assert.That(Unsafe.SizeOf<GameCardIdSet>(), Is.EqualTo(12));

        Assert.That(GetOffset(in s, in s.Id1), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.Id2), Is.EqualTo(4));
        Assert.That(GetOffset(in s, in s.Id3), Is.EqualTo(8));
    }

    [Test]
    public static void CardId1_Layout()
    {
        var s = new CardId1();

        Assert.That(Unsafe.SizeOf<CardId1>(), Is.EqualTo(4));

        Assert.That(GetOffset(in s, in s.MakerCode), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.MemoryCapacity), Is.EqualTo(1));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(2));
        Assert.That(GetOffset(in s, in s.MemoryType), Is.EqualTo(3));
    }

    [Test]
    public static void CardId2_Layout()
    {
        var s = new CardId2();

        Assert.That(Unsafe.SizeOf<CardId2>(), Is.EqualTo(4));

        Assert.That(GetOffset(in s, in s.CardSecurityNumber), Is.EqualTo(0));
        Assert.That(GetOffset(in s, in s.CardType), Is.EqualTo(1));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(2));
    }

    [Test]
    public static void CardId3_Layout()
    {
        var s = new CardId3();

        Assert.That(Unsafe.SizeOf<CardId3>(), Is.EqualTo(4));

        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0));
    }

    [Test]
    public static void CardInitialDataPayload_Layout()
    {
        var s = new CardInitialDataPayload();

        Assert.That(Unsafe.SizeOf<CardInitialDataPayload>(), Is.EqualTo(0x3C));

        Assert.That(GetOffset(in s, in s.PackageId), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.AuthData), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.AuthMac), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.AuthNonce), Is.EqualTo(0x30));
    }

    [Test]
    public static void DevCardParameter_Layout()
    {
        var s = new DevCardParameter();

        Assert.That(Unsafe.SizeOf<DevCardParameter>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.CardId1), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.CardId2), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.CardId3), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.RomAreaStartAddr), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.BackupAreaStartAddr), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.ReservedAreaStartAddr), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.RomSize), Is.EqualTo(0x17));
        Assert.That(GetOffset(in s, in s.WaitCycle1ForRead), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.WaitCycle2ForRead), Is.EqualTo(0x1A));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle1FrequencyForRead), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle1ForRead), Is.EqualTo(0x1D));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle2FrequencyForRead), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle2ForRead), Is.EqualTo(0x21));
        Assert.That(GetOffset(in s, in s.FirstReadPageWaitCycleForRead), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.WaitCycle1ForWrite), Is.EqualTo(0x27));
        Assert.That(GetOffset(in s, in s.WaitCycle2ForWrite), Is.EqualTo(0x29));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle1FrequencyForWrite), Is.EqualTo(0x2C));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle1ForWrite), Is.EqualTo(0x2D));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle2FrequencyForWrite), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.SpeedChangeEmulateWaitCycle2ForWrite), Is.EqualTo(0x31));
        Assert.That(GetOffset(in s, in s.WaitCycle1ForSetAccessPattern), Is.EqualTo(0x34));
        Assert.That(GetOffset(in s, in s.WaitCycle2ForSetAccessPattern), Is.EqualTo(0x36));
        Assert.That(GetOffset(in s, in s.WaitCycleForRefresh), Is.EqualTo(0x39));
        Assert.That(GetOffset(in s, in s.WaitCycleForSetKey), Is.EqualTo(0x3C));
        Assert.That(GetOffset(in s, in s.WaitCycleForIRdInit), Is.EqualTo(0x3F));
        Assert.That(GetOffset(in s, in s.WaitCycleForISetInit1), Is.EqualTo(0x42));
        Assert.That(GetOffset(in s, in s.WaitCycleForISetGen), Is.EqualTo(0x45));
        Assert.That(GetOffset(in s, in s.WaitCycleForISetInit2), Is.EqualTo(0x48));
        Assert.That(GetOffset(in s, in s.NandSize), Is.EqualTo(0x4B));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x4C));
    }

    [Test]
    public static void CardInitialData_Layout()
    {
        var s = new CardInitialData();

        Assert.That(Unsafe.SizeOf<CardInitialData>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.Payload), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.Padding), Is.EqualTo(0x3C));
    }

    [Test]
    public static void CardHeaderEncryptedData_Layout()
    {
        var s = new CardHeaderEncryptedData();

        Assert.That(Unsafe.SizeOf<CardHeaderEncryptedData>(), Is.EqualTo(0x70));

        Assert.That(GetOffset(in s, in s.FwVersion), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.AccCtrl1), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.Wait1TimeRead), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.Wait2TimeRead), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.Wait1TimeWrite), Is.EqualTo(0x14));
        Assert.That(GetOffset(in s, in s.Wait2TimeWrite), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.FwMode), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.CupVersion), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.CompatibilityType), Is.EqualTo(0x24));
        Assert.That(GetOffset(in s, in s.Reserved25), Is.EqualTo(0x25));
        Assert.That(GetOffset(in s, in s.Reserved26), Is.EqualTo(0x26));
        Assert.That(GetOffset(in s, in s.Reserved27), Is.EqualTo(0x27));
        Assert.That(GetOffset(in s, in s.UppHash), Is.EqualTo(0x28));
        Assert.That(GetOffset(in s, in s.CupId), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.Reserved38), Is.EqualTo(0x38));
    }

    [Test]
    public static void CardHeader_Layout()
    {
        var s = new CardHeader();

        Assert.That(Unsafe.SizeOf<CardHeader>(), Is.EqualTo(0x100));

        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.RomAreaStartPage), Is.EqualTo(0x04));
        Assert.That(GetOffset(in s, in s.BackupAreaStartPage), Is.EqualTo(0x08));
        Assert.That(GetOffset(in s, in s.KeyIndex), Is.EqualTo(0x0C));
        Assert.That(GetOffset(in s, in s.RomSize), Is.EqualTo(0x0D));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x0E));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x0F));
        Assert.That(GetOffset(in s, in s.PackageId), Is.EqualTo(0x10));
        Assert.That(GetOffset(in s, in s.ValidDataEndPage), Is.EqualTo(0x18));
        Assert.That(GetOffset(in s, in s.Reserved11C), Is.EqualTo(0x1C));
        Assert.That(GetOffset(in s, in s.Iv), Is.EqualTo(0x20));
        Assert.That(GetOffset(in s, in s.PartitionFsHeaderAddress), Is.EqualTo(0x30));
        Assert.That(GetOffset(in s, in s.PartitionFsHeaderSize), Is.EqualTo(0x38));
        Assert.That(GetOffset(in s, in s.PartitionFsHeaderHash), Is.EqualTo(0x40));
        Assert.That(GetOffset(in s, in s.InitialDataHash), Is.EqualTo(0x60));
        Assert.That(GetOffset(in s, in s.SelSec), Is.EqualTo(0x80));
        Assert.That(GetOffset(in s, in s.SelT1Key), Is.EqualTo(0x84));
        Assert.That(GetOffset(in s, in s.SelKey), Is.EqualTo(0x88));
        Assert.That(GetOffset(in s, in s.LimAreaPage), Is.EqualTo(0x8C));
        Assert.That(GetOffset(in s, in s.EncryptedData), Is.EqualTo(0x90));
    }

    [Test]
    public static void CardHeaderWithSignature_Layout()
    {
        var s = new CardHeaderWithSignature();

        Assert.That(Unsafe.SizeOf<CardHeaderWithSignature>(), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.Signature), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Data), Is.EqualTo(0x100));
    }

    [Test]
    public static void T1CardCertificate_Layout()
    {
        var s = new T1CardCertificate();

        Assert.That(Unsafe.SizeOf<T1CardCertificate>(), Is.EqualTo(0x400));

        Assert.That(GetOffset(in s, in s.Signature), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Magic), Is.EqualTo(0x100));
        Assert.That(GetOffset(in s, in s.Version), Is.EqualTo(0x104));
        Assert.That(GetOffset(in s, in s.KekIndex), Is.EqualTo(0x108));
        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x109));
        Assert.That(GetOffset(in s, in s.T1CardDeviceId), Is.EqualTo(0x110));
        Assert.That(GetOffset(in s, in s.Iv), Is.EqualTo(0x120));
        Assert.That(GetOffset(in s, in s.HwKey), Is.EqualTo(0x130));
        Assert.That(GetOffset(in s, in s.Reserved), Is.EqualTo(0x140));
        Assert.That(GetOffset(in s, in s.Padding), Is.EqualTo(0x200));
    }

    [Test]
    public static void Ca10Certificate_Layout()
    {
        var s = new Ca10Certificate();

        Assert.That(Unsafe.SizeOf<Ca10Certificate>(), Is.EqualTo(0x400));

        Assert.That(GetOffset(in s, in s.Signature), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Unk100), Is.EqualTo(0x100));
        Assert.That(GetOffset(in s, in s.Modulus), Is.EqualTo(0x130));
        Assert.That(GetOffset(in s, in s.Unk230), Is.EqualTo(0x230));
    }
}