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
    #if IS_TPM_BYPASS_ENABLED
    #pragma warning disable CS0618 // Type or member is obsolete

    private static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hyjinx"); 
    private static readonly string SystemPath = Path.Combine(RootPath, "system");
    
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

        var ncaFileName = "00b7c40c749108f42bdebad952179172.nca";
        var ncaFilePath = Path.Combine(RootPath, "bis", "system", "Contents", "registered", ncaFileName, "00");
        if (!File.Exists(ncaFilePath))
        {
            Assert.Fail($"The file '{ncaFilePath}' does not exist.");
        }

        using var fs = File.OpenRead(ncaFilePath);
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
