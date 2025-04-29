#if IS_TPM_BYPASS_ENABLED

using Hyjinx.Common.Configuration;
using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using System.IO;
using Xunit;
using Path = System.IO.Path;

namespace LibHac.Tests.Tools;

public class NspTests : DecrypterTests
{
    [Fact]
    public void CanReadApplicationDataFromEncryptedNsp()
    {
        AppDataManager.Initialize("./");
        
        var file = new FileInfo(Path.Combine(SourceGamesPath, "Baba Is You [01002CD00A51C000][v0][Base].nsp"));
        if (!file.Exists)
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }

        var vfs = VirtualFileSystem.CreateInstance(CreateEncryptedKeySet());
        var library = new ApplicationLibrary(vfs, IntegrityCheckLevel.ErrorOnInvalid);

        using var fs = file.OpenRead();

        var pfs = new PartitionFileSystem();
        pfs.Initialize(fs.AsStorage());

        var result = library.GetApplicationFromNsp(pfs, file.FullName);
        Assert.Equal("Baba Is You", result.Name);
    }
}

#endif
