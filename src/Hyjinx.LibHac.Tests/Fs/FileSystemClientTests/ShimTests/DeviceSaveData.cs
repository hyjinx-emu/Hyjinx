using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSrv.Impl;
using NUnit.Framework;

namespace LibHac.Tests.Fs.FileSystemClientTests.ShimTests;

public class DeviceSaveData
{
    [Test]
    public static void MountDeviceSaveData_SaveDoesNotExist_ReturnsTargetNotFound()
    {
        var applicationId = new Ncm.ApplicationId(1234);
        HorizonServerSet hos = FileSystemServerFactory.CreateHorizon(applicationId, fsAcBits: AccessControlBits.Bits.FullPermission);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.TargetNotFound, hos.Client.Fs.MountDeviceSaveData("device"u8, applicationId));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.TargetNotFound, hos.Client.Fs.MountDeviceSaveData("device2"u8));
    }

    [Test]
    public static void MountDeviceSaveData_OwnDeviceSaveExists_ReturnsSuccess()
    {
        var applicationId = new Ncm.ApplicationId(1234);
        HorizonServerSet hos = FileSystemServerFactory.CreateHorizon(applicationId, fsAcBits: AccessControlBits.Bits.FullPermission);

        LibHac.Tests.Fs.FsTestAssert.Success(hos.Client.Fs.CreateDeviceSaveData(applicationId, applicationId.Value, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(hos.Client.Fs.MountDeviceSaveData("device"u8));
        LibHac.Tests.Fs.FsTestAssert.Success(hos.Client.Fs.MountDeviceSaveData("device2"u8, applicationId));
    }

    [Test]
    public static void MountDeviceSaveData_OtherDeviceSaveExists_ReturnsSuccess()
    {
        var ownApplicationId = new Ncm.ApplicationId(1234);
        var otherApplicationId = new Ncm.ApplicationId(12345);
        HorizonServerSet hos = FileSystemServerFactory.CreateHorizon(ownApplicationId, fsAcBits: AccessControlBits.Bits.FullPermission);

        LibHac.Tests.Fs.FsTestAssert.Success(hos.Client.Fs.CreateDeviceSaveData(otherApplicationId, otherApplicationId.Value, 0, 0, SaveDataFlags.None));
        LibHac.Tests.Fs.FsTestAssert.Success(hos.Client.Fs.MountDeviceSaveData("device"u8, otherApplicationId));

        // Try to open missing own device save data
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.TargetNotFound, hos.Client.Fs.MountDeviceSaveData("device2"u8));
    }

    [Test]
    public static void IsDeviceSaveDataExisting_ReturnsCorrectState()
    {
        var applicationId = new Ncm.ApplicationId(1234);
        var ncmApplicationId = new Ncm.ApplicationId(applicationId.Value);
        HorizonServerSet hos = FileSystemServerFactory.CreateHorizon(ncmApplicationId);
        FileSystemClient fs = hos.InitialProcessClient.Fs;

        // Should return false when there aren't any saves.
        Assert.That(fs.IsDeviceSaveDataExisting(applicationId), Is.False);

        // Should return true after creating the save.
        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateDeviceSaveData(ncmApplicationId, applicationId.Value, 0, 0, SaveDataFlags.None));
        Assert.That(fs.IsDeviceSaveDataExisting(applicationId), Is.True);

        // Should return false after deleting the save.
        LibHac.Tests.Fs.FsTestAssert.Success(fs.DeleteDeviceSaveData(applicationId));
        Assert.That(fs.IsDeviceSaveDataExisting(applicationId), Is.False);
    }
}