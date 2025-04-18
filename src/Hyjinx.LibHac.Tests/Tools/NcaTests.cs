#pragma warning disable CS0618 // Type or member is obsolete

using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using System.IO;
using Xunit;
using Path = System.IO.Path;

namespace LibHac.Tests.Tools;

public class NcaTests
{
    #region Constants
    
    /// <summary>
    /// Defines the root path.
    /// </summary>
    private static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hyjinx");
    
    /// <summary>
    /// Defines the full path to the 'system' folder.
    /// </summary>
    private static readonly string SystemPath = Path.Combine(RootPath, "system");
    
    /// <summary>
    /// Defines the full path to the 'registered' folder.
    /// </summary>
    private static readonly string RegisteredPath = Path.Combine(RootPath, "bis", "system", "Contents", "registered");

    /// <summary>
    /// Defines the name of the NCA file to target.
    /// </summary>
    private static string NcaFileName = "00b7c40c749108f42bdebad952179172.nca";
    
    /// <summary>
    /// Defines the location of an encrypted NCA file.
    /// </summary>
    private static readonly string EncryptedNcaFile = Path.Combine(RegisteredPath, NcaFileName);
    
    /// <summary>
    /// Defines the location of an unencrypted NCA file.
    /// </summary>
    private static readonly string UnencryptedNcaFile = Path.Combine(RegisteredPath, $"{NcaFileName}.unencrypted");
    
    #endregion
    
    #if IS_TPM_BYPASS_ENABLED
    
    [Fact]
    public void CanReadAnEncryptedNca()
    {
        var keySet = CreateEncryptedKeySet();

        var file = Path.Combine(EncryptedNcaFile, "00");
        if (!File.Exists(file))
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }
        
        using var fs = File.OpenRead(file);
        var target = new Nca(keySet, fs.AsStorage());

        var result = target.VerifyNca();
        Assert.True(result == Validity.Valid);
    }

    private KeySet CreateEncryptedKeySet()
    {
        var prodKeysFile = GetFileIfExists(SystemPath, "prod.keys");
        if (prodKeysFile == null)
        {
            Assert.Fail("The prod.keys file must be present to run this test.");
        }
        
        var titleKeysFile = GetFileIfExists(SystemPath, "title.keys");
        var consoleKeysFile = GetFileIfExists(SystemPath, "console.keys");
        
        var keySet = KeySet.CreateDefaultKeySet();
        ExternalKeyReader.ReadKeyFile(keySet, prodKeysFile, titleKeysFile, consoleKeysFile);
        
        return keySet;
    }

    private static string? GetFileIfExists(string path, string fileName)
    {
        var fullPath = Path.Combine(path, fileName);
        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        return null;
    }
    
    #endif

    [Fact]
    public void CanDecryptAnEncryptedNcaFile()
    {
        var keySet = CreateEncryptedKeySet();

        var inFile = Path.Combine(EncryptedNcaFile, "00");
        var outFile = Path.Combine(UnencryptedNcaFile, "00");
        
        using var fs = File.OpenRead(inFile);
        var target = new Nca(keySet, fs.AsStorage());

        var horizon = new Horizon(new HorizonConfiguration());
        
        var client = horizon.CreatePrivilegedHorizonClient();
        client.Fs.SetServerlessAccessLog(true);
        
        if (Directory.Exists(UnencryptedNcaFile))
        {
            Directory.Delete(UnencryptedNcaFile, true);
        }

        Directory.CreateDirectory(UnencryptedNcaFile);

        using var outStream = File.OpenWrite(outFile);
        target.OpenDecryptedHeaderStorage().Slice(0, NcaHeader.HeaderSize).CopyToStream(outStream);
        outStream.Flush();
        
        Assert.Success(Result.Success);
    }
    
    [Fact]
    public void CanReadAnUnencryptedNca()
    {
        var file = Path.Combine(UnencryptedNcaFile, "00");
        if (!File.Exists(file))
        {
            Assert.Fail($"The file '{file}' does not exist.");
        }

        using var fs = File.OpenRead(file);
        var target = new Nca(KeySet.Empty, fs.AsStorage());

        var result = target.VerifyNca();
        Assert.True(result == Validity.Valid);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
