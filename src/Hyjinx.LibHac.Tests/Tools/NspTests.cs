#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using Hyjinx.Common.Configuration;
using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.IO;
using Xunit;
using Path = System.IO.Path;

namespace LibHac.Tests.Tools;

public class NspTests : DecrypterTests
{
    /// <summary>
    /// Defines the name of the NSP file to target.
    /// </summary>
    private static string NspFileName = "Baba Is You [01002CD00A51C000][v0][Base].nsp";
    
    /// <summary>
    /// Defines the location of an encrypted NSP file.
    /// </summary>
    private static readonly string EncryptedNspFile = Path.Combine(SourceGamesPath, NspFileName);

    static NspTests()
    {
        AppDataManager.Initialize("./");
    }
    
    [Fact]
    public void CanReadApplicationDataFromEncryptedNsp()
    {
        var file = new FileInfo(EncryptedNspFile);
        if (!file.Exists)
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }

        var keySet = CreateEncryptedKeySet();
        
        var result = ReadApplicationData(keySet, file);
        Assert.Equal("Baba Is You", result.Name);
    }

    private static ApplicationData ReadApplicationData(KeySet keySet, FileInfo file)
    {
        using var fs = file.OpenRead();

        var pfs = new PartitionFileSystem();
        pfs.Initialize(fs.AsStorage());
        
        using var vfs = VirtualFileSystem.CreateInstance(keySet, true);
        var library = new ApplicationLibrary(vfs, IntegrityCheckLevel.ErrorOnInvalid);

        return library.GetApplicationFromNsp(pfs, file.FullName);
    }
    
    [Fact]
    public void CanDecryptAnEncryptedNsp()
    {
        var inFile = new FileInfo(EncryptedNspFile);
        if (!inFile.Exists)
        {
            Assert.Fail($"The file '{inFile}' does not exist.");
        }

        var outFile = new FileInfo(Path.Combine("./", NspFileName));
        if (outFile.Exists)
        {
            outFile.Delete();
        }

        DoDecrypt(inFile, outFile);

        var data = ReadApplicationData(KeySet.Empty, outFile);
        Assert.Equal("Baba Is You", data.Name);
    }

    private static void DoDecrypt(FileInfo inFile, FileInfo destination)
    {
        var keySet = CreateEncryptedKeySet();
        using var inStream = inFile.OpenRead();

        var pfs = new PartitionFileSystem();
        pfs.Initialize(inStream.AsStorage());
        
        using var outStream = destination.OpenWrite();
        
        var decrypter = new NspDecrypter(keySet);
        decrypter.Decrypt(pfs, outStream);

        outStream.Flush();
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
