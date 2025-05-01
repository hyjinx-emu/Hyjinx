#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using Hyjinx.Common.Configuration;
using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
using Xunit;
using Path = System.IO.Path;

namespace LibHac.Tests.Tools;

public class NspTests : DecrypterTests
{
    /// <summary>
    /// Defines the name of the NSP file to target.
    /// </summary>
    private static string NspFileName = "Baba Is You [01002CD00A51C800][v720896][Update].nsp";
    
    /// <summary>
    /// Defines the path to the source emulation roms.
    /// </summary>
    protected static readonly string SourceGamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Emulation", "Hyjinx", "backup");
    
    /// <summary>
    /// Defines the path to the destination emulation roms.
    /// </summary>
    protected static readonly string DestinationGamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Emulation", "Hyjinx", "roms");
    
    /// <summary>
    /// Defines the location of an encrypted NSP file.
    /// </summary>
    private static readonly string EncryptedNspFile = Path.Combine(SourceGamesPath, NspFileName);

    /// <summary>
    /// Defines the location of a decrypted NSP file.
    /// </summary>
    private static readonly string DecryptedNspFile = Path.Combine(DestinationGamesPath, NspFileName);
    
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
    
    [Fact]
    public void CanReadApplicationDataFromDecryptedNsp()
    {
        var file = new FileInfo(DecryptedNspFile);
        if (!file.Exists)
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }
        
        var result = ReadApplicationData(KeySet.Empty, file);
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

        var outFile = new FileInfo(DecryptedNspFile);
        if (outFile.Exists)
        {
            outFile.Delete();
        }

        DoDecrypt(inFile, outFile);
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
