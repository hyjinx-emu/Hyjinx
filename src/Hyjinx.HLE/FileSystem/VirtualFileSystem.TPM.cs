#if IS_TPM_BYPASS_ENABLED
#pragma warning disable CS0618 // Type or member is obsolete

using Hyjinx.Common.Configuration;
using LibHac.Common.Keys;
using System;
using System.IO;

namespace Hyjinx.HLE.FileSystem;

partial class VirtualFileSystem
{
    [Obsolete("This method can no longer be used due to TPM restrictions.")]
    public void ReloadKeySet()
    {
        KeySet ??= KeySet.CreateDefaultKeySet();
        
        string keyFile = null;
        string titleKeyFile = null;
        string consoleKeyFile = null;
        
        if (AppDataManager.Mode == AppDataManager.LaunchMode.UserProfile)
        {
            LoadSetAtPath(AppDataManager.KeysDirPathUser);
        }
        
        LoadSetAtPath(AppDataManager.KeysDirPath);
        
        void LoadSetAtPath(string basePath)
        {
            string localKeyFile = Path.Combine(basePath, "prod.keys");
            string localTitleKeyFile = Path.Combine(basePath, "title.keys");
            string localConsoleKeyFile = Path.Combine(basePath, "console.keys");
        
            if (File.Exists(localKeyFile))
            {
                keyFile = localKeyFile;
            }
        
            if (File.Exists(localTitleKeyFile))
            {
                titleKeyFile = localTitleKeyFile;
            }
        
            if (File.Exists(localConsoleKeyFile))
            {
                consoleKeyFile = localConsoleKeyFile;
            }
        }
        
        ExternalKeyReader.ReadKeyFile(KeySet, keyFile, titleKeyFile, consoleKeyFile, null);
    }   
}

#pragma warning restore CS0618 // Type or member is obsolete
#endif
