using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;
using NUnit.Framework;

namespace LibHac.Tests.Fs.FileSystemClientTests.ShimTests;

public class BcatSaveData
{
    [Test]
    public void MountBcatSaveData_SaveDoesNotExist_ReturnsTargetNotFound()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.TargetNotFound, fs.MountBcatSaveData("bcat_test"u8, applicationId));
    }

    [Test]
    public void MountBcatSaveData_SaveExists_ReturnsSuccess()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateBcatSaveData(applicationId, 0x400000));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountBcatSaveData("bcat_test"u8, applicationId));
    }

    [Test]
    public void MountBcatSaveData_WrittenDataPersists()
    {
        var applicationId = new Ncm.ApplicationId(1);
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.CreateBcatSaveData(applicationId, 0x400000));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountBcatSaveData("bcat_test"u8, applicationId));

        // Check that the path doesn't exist
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PathNotFound, fs.GetEntryType(out _, "bcat_test:/file"u8));

        fs.CreateFile("bcat_test:/file"u8, 0);
        fs.Commit("bcat_test"u8);
        fs.Unmount("bcat_test"u8);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountBcatSaveData("bcat_test"u8, applicationId));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out DirectoryEntryType type, "bcat_test:/file"u8));
        Assert.That(type, Is.EqualTo(DirectoryEntryType.File));
    }
}