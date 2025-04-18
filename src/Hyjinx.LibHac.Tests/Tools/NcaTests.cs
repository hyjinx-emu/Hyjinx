using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using System.IO;
using Xunit;

namespace LibHac.Tests.Tools;

public class NcaTests
{
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
    /// Defines the file which will be the target of the tests.
    /// </summary>
    private static readonly string NcaFile = Path.Combine(RegisteredPath, "00b7c40c749108f42bdebad952179172.nca", "00");
    
    #if IS_TPM_BYPASS_ENABLED
    #pragma warning disable CS0618 // Type or member is obsolete
    
    [Fact]
    public void CanReadAnEncryptedNca()
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
        
        if (!File.Exists(NcaFile))
        {
            Assert.Fail($"The file '{NcaFile}' does not exist.");
        }

        using var fs = File.OpenRead(NcaFile);
        var target = new Nca(keySet, fs.AsStorage());

        var result = target.VerifyNca();
        Assert.True(result == Validity.Valid);
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
    
    #pragma warning restore CS0618 // Type or member is obsolete
    #endif
}
