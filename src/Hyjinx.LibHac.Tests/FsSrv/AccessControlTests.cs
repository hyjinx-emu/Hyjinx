using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSrv.Impl;
using LibHac.Ncm;
using ContentType = LibHac.Fs.ContentType;

namespace LibHac.Tests.FsSrv;

public class AccessControlTests
{
    [Test]
    public void OpenFileSystemWithNoPermissions_ReturnsPermissionDenied()
    {
        Horizon hos = HorizonFactory.CreateBasicHorizon();

        HorizonClient regClient = hos.CreatePrivilegedHorizonClient();
        HorizonClient client = hos.CreateHorizonClient();

        var dataHeader = new AccessControlDataHeader();
        var descriptor = new AccessControlDescriptor();

        descriptor.Version = 123;
        dataHeader.Version = 123;

        descriptor.AccessFlags = (ulong)AccessControlBits.Bits.None;
        dataHeader.AccessFlags = (ulong)AccessControlBits.Bits.None;

        Result registerResult = regClient.Fs.RegisterProgram(client.ProcessId.Value, new ProgramId(123),
            StorageId.BuiltInUser, SpanHelpers.AsReadOnlyByteSpan(in dataHeader),
            SpanHelpers.AsReadOnlyByteSpan(in descriptor));
        Assert.That(registerResult.IsSuccess(), Is.True, registerResult.ToString());

        Result res = client.Fs.MountContent("test"u8, "@System:/fake.nca"u8, ContentType.Meta);
        Assert.That(ResultFs.PermissionDenied.Includes(res), Is.True, res.ToString());
    }

    [Test]
    public void OpenFileSystemWithPermissions_ReturnsInvalidNcaMountPoint()
    {
        Horizon hos = HorizonFactory.CreateBasicHorizon();

        HorizonClient regClient = hos.CreatePrivilegedHorizonClient();
        HorizonClient client = hos.CreateHorizonClient();

        var dataHeader = new AccessControlDataHeader();
        var descriptor = new AccessControlDescriptor();

        descriptor.Version = 123;
        dataHeader.Version = 123;

        descriptor.AccessFlags = (ulong)AccessControlBits.Bits.ApplicationInfo;
        dataHeader.AccessFlags = (ulong)AccessControlBits.Bits.ApplicationInfo;

        Result registerResult = regClient.Fs.RegisterProgram(client.ProcessId.Value, new ProgramId(123),
            StorageId.BuiltInUser, SpanHelpers.AsReadOnlyByteSpan(in dataHeader),
            SpanHelpers.AsReadOnlyByteSpan(in descriptor));
        Assert.That(registerResult.IsSuccess(), Is.True, registerResult.ToString());

        // We should get UnexpectedInNcaFileSystemServiceImplA because mounting NCAs from @System isn't allowed
        Result res = client.Fs.MountContent("test"u8, "@System:/fake.nca"u8, ContentType.Meta);
        Assert.That(ResultFs.UnexpectedInNcaFileSystemServiceImplA.Includes(res), Is.True, res.ToString());
    }
}