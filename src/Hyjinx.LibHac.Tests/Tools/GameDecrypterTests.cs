#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using Hyjinx.Common.Configuration;
using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
using Xunit;

namespace LibHac.Tests.Tools;

/// <summary>
/// A decrypter test for game content files.
/// </summary>
/// <remarks>Reads from a source other than where Hyjinx is looking for content so only already decrypted content is visible.</remarks>
public abstract class GameDecrypterTests : DecrypterTests
{
    protected abstract string TargetFileName { get; }
    
    protected abstract string TitleName { get; }
    
    /// <summary>
    /// Defines the path to the source emulation roms.
    /// </summary>
    protected static readonly string SourceGamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Emulation", "Hyjinx", "backup");
    
    /// <summary>
    /// Defines the path to the destination emulation roms.
    /// </summary>
    protected static readonly string DestinationGamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Emulation", "Hyjinx", "roms");
    
    static GameDecrypterTests()
    {
        AppDataManager.Initialize("./");
    }

    private FileInfo GetEncryptedFile()
    {
        return new FileInfo(Path.Combine(SourceGamesPath, TargetFileName));
    }
    
    private FileInfo GetDecryptedFile()
    {
        return new FileInfo(Path.Combine(DestinationGamesPath, TargetFileName));
    }
    
    [Fact]
    public void CanReadApplicationDataFromEncryptedFile()
    {
        var file = GetEncryptedFile();
        if (!file.Exists)
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }

        var keySet = CreateEncryptedKeySet();
        
        var result = ReadApplicationData(keySet, file);
        Assert.Equal(TitleName, result.Name);
    }
    
    [Fact]
    public void CanReadApplicationDataFromDecryptedFile()
    {
        var file = GetDecryptedFile();
        if (!file.Exists)
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }
        
        var result = ReadApplicationData(KeySet.Empty, file);
        Assert.Equal(TitleName, result.Name);
    }

    protected abstract ApplicationData ReadApplicationData(KeySet keySet, FileInfo file);
    
    [Fact]
    public void CanDecryptAnEncryptedFile()
    {
        var inFile = GetEncryptedFile();
        if (!inFile.Exists)
        {
            Assert.Fail($"The file '{inFile}' does not exist.");
        }

        var outFile = GetDecryptedFile();
        if (outFile.Exists)
        {
            outFile.Delete();
        }

        DoDecrypt(inFile, outFile);
    }

    protected abstract void DoDecrypt(FileInfo inFile, FileInfo destination);
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
