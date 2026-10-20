using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.Ncm;
using NUnit.Framework;

namespace LibHac.Tests.Fs.FileSystemClientTests.ShimTests;

public class Debug
{
    [Test]
    public void SetDebugOption_KeyIsZero_Aborts()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        Assert.Throws<HorizonResultException>(() => fs.SetDebugOption(0, 1));
    }

    [Test]
    public void SetDebugOption_NoPermissions_ReturnsPermissionDenied()
    {
        Horizon hos = HorizonFactory.CreateBasicHorizon();

        HorizonClient client =
            hos.CreateHorizonClient(new ProgramLocation(new ProgramId(1), StorageId.BuiltInSystem), 0);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PermissionDenied, client.Fs.SetDebugOption((DebugOptionKey)1, 0));
    }

    [Test]
    public void SetDebugOption_DebugConfigIsFull_Aborts()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)1, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)2, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)3, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)4, 0));

        Assert.Throws<LibHacException>(() => fs.SetDebugOption((DebugOptionKey)5, 0));
    }

    [Test]
    public void SetDebugOption_ReplaceExistingValueWhenFull_ReturnsSuccess()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)1, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)2, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)3, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)4, 0));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)1, 10));
    }

    [Test]
    public void SetDebugOption_AfterRemovingKeyWhenFull_ReturnsSuccess()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)1, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)2, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)3, 0));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)4, 0));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.UnsetDebugOption((DebugOptionKey)2));

        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption((DebugOptionKey)2, 4));
    }

    [Test]
    public void UnsetDebugOption_UnsetExistingKey_ReturnsSuccess()
    {
        const DebugOptionKey key = DebugOptionKey.SaveDataEncryption;
        const long value = 0;

        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.SetDebugOption(key, value));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.UnsetDebugOption(key));
    }

    [Test]
    public void UnsetDebugOption_NoPermissions_ReturnsPermissionDenied()
    {
        Horizon hos = HorizonFactory.CreateBasicHorizon();

        HorizonClient client =
            hos.CreateHorizonClient(new ProgramLocation(new ProgramId(1), StorageId.BuiltInSystem), 0);

        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.PermissionDenied, client.Fs.UnsetDebugOption((DebugOptionKey)1));
    }
}