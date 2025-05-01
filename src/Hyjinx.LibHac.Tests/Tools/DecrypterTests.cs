#if IS_TPM_BYPASS_ENABLED
#pragma warning disable CS0618 // Type or member is obsolete

using LibHac.Common.Keys;
using System;
using System.IO;
using Xunit;

namespace LibHac.Tests.Tools;

public class DecrypterTests
{
    /// <summary>
    /// Defines the full path to the 'system' folder.
    /// </summary>
    protected static readonly string SystemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");
    
    protected static KeySet CreateEncryptedKeySet()
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
}

#pragma warning disable CS0618 // Type or member is obsolete
#endif
