using LibHac.Ns;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using static LibHac.Ns.ApplicationControlProperty;
using static LibHac.Tests.Common.Layout;

namespace LibHac.Tests.Ns;

public class TypeLayoutTests
{
    [Test]
    public static void ApplicationTitle_Layout()
    {
        var s = new ApplicationTitle();

        Assert.That(Unsafe.SizeOf<ApplicationTitle>(), Is.EqualTo(0x300));

        Assert.That(GetOffset(in s, in s.Name), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.Publisher), Is.EqualTo(0x200));

        Assert.That(GetOffset(in s, in s.NameString.Value[0]), Is.EqualTo(0x000));
        Assert.That(GetOffset(in s, in s.PublisherString.Value[0]), Is.EqualTo(0x200));
    }

    [Test]
    public static void ApplicationNeighborDetectionGroupConfiguration_Layout()
    {
        var s = new ApplicationNeighborDetectionGroupConfiguration();

        Assert.That(Unsafe.SizeOf<ApplicationNeighborDetectionGroupConfiguration>(), Is.EqualTo(0x18));

        Assert.That(GetOffset(in s, in s.GroupId), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.Key), Is.EqualTo(0x8));
    }

    [Test]
    public static void ApplicationNeighborDetectionClientConfiguration_Layout()
    {
        var s = new ApplicationNeighborDetectionClientConfiguration();

        Assert.That(Unsafe.SizeOf<ApplicationNeighborDetectionClientConfiguration>(), Is.EqualTo(0x198));

        Assert.That(GetOffset(in s, in s.SendGroupConfiguration), Is.EqualTo(0x00));
        Assert.That(GetOffset(in s, in s.ReceivableGroupConfigurations), Is.EqualTo(0x18));
    }

    [Test]
    public static void ApplicationJitConfiguration_Layout()
    {
        var s = new ApplicationJitConfiguration();

        Assert.That(Unsafe.SizeOf<ApplicationJitConfiguration>(), Is.EqualTo(0x10));

        Assert.That(GetOffset(in s, in s.Flags), Is.EqualTo(0x0));
        Assert.That(GetOffset(in s, in s.MemorySize), Is.EqualTo(0x8));
    }

    [Test]
    public static void ApplicationControlProperty_Layout()
    {
        var s = new ApplicationControlProperty();

        Assert.That(Unsafe.SizeOf<ApplicationControlProperty>(), Is.EqualTo(0x4000));

        Assert.That(GetOffset(in s, in s.Title), Is.EqualTo(0x0000));
        Assert.That(GetOffset(in s, in s.Isbn), Is.EqualTo(0x3000));
        Assert.That(GetOffset(in s, in s.StartupUserAccount), Is.EqualTo(0x3025));
        Assert.That(GetOffset(in s, in s.UserAccountSwitchLock), Is.EqualTo(0x3026));
        Assert.That(GetOffset(in s, in s.AddOnContentRegistrationType), Is.EqualTo(0x3027));
        Assert.That(GetOffset(in s, in s.AttributeFlag), Is.EqualTo(0x3028));
        Assert.That(GetOffset(in s, in s.SupportedLanguageFlag), Is.EqualTo(0x302C));
        Assert.That(GetOffset(in s, in s.ParentalControlFlag), Is.EqualTo(0x3030));
        Assert.That(GetOffset(in s, in s.Screenshot), Is.EqualTo(0x3034));
        Assert.That(GetOffset(in s, in s.VideoCapture), Is.EqualTo(0x3035));
        Assert.That(GetOffset(in s, in s.DataLossConfirmation), Is.EqualTo(0x3036));
        Assert.That(GetOffset(in s, in s.PlayLogPolicy), Is.EqualTo(0x3037));
        Assert.That(GetOffset(in s, in s.PresenceGroupId), Is.EqualTo(0x3038));
        Assert.That(GetOffset(in s, in s.RatingAge), Is.EqualTo(0x3040));
        Assert.That(GetOffset(in s, in s.DisplayVersion), Is.EqualTo(0x3060));
        Assert.That(GetOffset(in s, in s.AddOnContentBaseId), Is.EqualTo(0x3070));
        Assert.That(GetOffset(in s, in s.SaveDataOwnerId), Is.EqualTo(0x3078));
        Assert.That(GetOffset(in s, in s.UserAccountSaveDataSize), Is.EqualTo(0x3080));
        Assert.That(GetOffset(in s, in s.UserAccountSaveDataJournalSize), Is.EqualTo(0x3088));
        Assert.That(GetOffset(in s, in s.DeviceSaveDataSize), Is.EqualTo(0x3090));
        Assert.That(GetOffset(in s, in s.DeviceSaveDataJournalSize), Is.EqualTo(0x3098));
        Assert.That(GetOffset(in s, in s.BcatDeliveryCacheStorageSize), Is.EqualTo(0x30A0));
        Assert.That(GetOffset(in s, in s.ApplicationErrorCodeCategory), Is.EqualTo(0x30A8));
        Assert.That(GetOffset(in s, in s.LocalCommunicationId), Is.EqualTo(0x30B0));
        Assert.That(GetOffset(in s, in s.LogoType), Is.EqualTo(0x30F0));
        Assert.That(GetOffset(in s, in s.LogoHandling), Is.EqualTo(0x30F1));
        Assert.That(GetOffset(in s, in s.RuntimeAddOnContentInstall), Is.EqualTo(0x30F2));
        Assert.That(GetOffset(in s, in s.RuntimeParameterDelivery), Is.EqualTo(0x30F3));
        Assert.That(GetOffset(in s, in s.Reserved30F4), Is.EqualTo(0x30F4));
        Assert.That(GetOffset(in s, in s.CrashReport), Is.EqualTo(0x30F6));
        Assert.That(GetOffset(in s, in s.Hdcp), Is.EqualTo(0x30F7));
        Assert.That(GetOffset(in s, in s.SeedForPseudoDeviceId), Is.EqualTo(0x30F8));
        Assert.That(GetOffset(in s, in s.BcatPassphrase), Is.EqualTo(0x3100));
        Assert.That(GetOffset(in s, in s.StartupUserAccountOption), Is.EqualTo(0x3141));
        Assert.That(GetOffset(in s, in s.ReservedForUserAccountSaveDataOperation), Is.EqualTo(0x3142));
        Assert.That(GetOffset(in s, in s.UserAccountSaveDataSizeMax), Is.EqualTo(0x3148));
        Assert.That(GetOffset(in s, in s.UserAccountSaveDataJournalSizeMax), Is.EqualTo(0x3150));
        Assert.That(GetOffset(in s, in s.DeviceSaveDataSizeMax), Is.EqualTo(0x3158));
        Assert.That(GetOffset(in s, in s.DeviceSaveDataJournalSizeMax), Is.EqualTo(0x3160));
        Assert.That(GetOffset(in s, in s.TemporaryStorageSize), Is.EqualTo(0x3168));
        Assert.That(GetOffset(in s, in s.CacheStorageSize), Is.EqualTo(0x3170));
        Assert.That(GetOffset(in s, in s.CacheStorageJournalSize), Is.EqualTo(0x3178));
        Assert.That(GetOffset(in s, in s.CacheStorageDataAndJournalSizeMax), Is.EqualTo(0x3180));
        Assert.That(GetOffset(in s, in s.CacheStorageIndexMax), Is.EqualTo(0x3188));
        Assert.That(GetOffset(in s, in s.Reserved318A), Is.EqualTo(0x318A));
        Assert.That(GetOffset(in s, in s.RuntimeUpgrade), Is.EqualTo(0x318B));
        Assert.That(GetOffset(in s, in s.SupportingLimitedLicenses), Is.EqualTo(0x318C));
        Assert.That(GetOffset(in s, in s.PlayLogQueryableApplicationId), Is.EqualTo(0x3190));
        Assert.That(GetOffset(in s, in s.PlayLogQueryCapability), Is.EqualTo(0x3210));
        Assert.That(GetOffset(in s, in s.RepairFlag), Is.EqualTo(0x3211));
        Assert.That(GetOffset(in s, in s.ProgramIndex), Is.EqualTo(0x3212));
        Assert.That(GetOffset(in s, in s.RequiredNetworkServiceLicenseOnLaunchFlag), Is.EqualTo(0x3213));
        Assert.That(GetOffset(in s, in s.Reserved3214), Is.EqualTo(0x3214));
        Assert.That(GetOffset(in s, in s.NeighborDetectionClientConfiguration), Is.EqualTo(0x3218));
        Assert.That(GetOffset(in s, in s.JitConfiguration), Is.EqualTo(0x33B0));
        Assert.That(GetOffset(in s, in s.RequiredAddOnContentsSetBinaryDescriptors), Is.EqualTo(0x33C0));
        Assert.That(GetOffset(in s, in s.PlayReportPermission), Is.EqualTo(0x3400));
        Assert.That(GetOffset(in s, in s.CrashScreenshotForProd), Is.EqualTo(0x3401));
        Assert.That(GetOffset(in s, in s.CrashScreenshotForDev), Is.EqualTo(0x3402));
        Assert.That(GetOffset(in s, in s.ContentsAvailabilityTransitionPolicy), Is.EqualTo(0x3403));
        Assert.That(GetOffset(in s, in s.Reserved3404), Is.EqualTo(0x3404));
        Assert.That(GetOffset(in s, in s.AccessibleLaunchRequiredVersion), Is.EqualTo(0x3408));
        Assert.That(GetOffset(in s, in s.Reserved3448), Is.EqualTo(0x3448));
    }
}