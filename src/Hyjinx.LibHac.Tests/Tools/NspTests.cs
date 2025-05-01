#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.IO;

namespace LibHac.Tests.Tools;

public class NspTests : GameDecrypterTests
{
    protected override string TargetFileName => "Baba Is You [01002CD00A51C800][v720896][Update].nsp";

    protected override string TitleName => "Baba Is You";

    protected override void DoDecrypt(FileInfo inFile, FileInfo destination)
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
    
    protected override ApplicationData ReadApplicationData(KeySet keySet, FileInfo file)
    {
        using var fs = file.OpenRead();

        var pfs = new PartitionFileSystem();
        pfs.Initialize(fs.AsStorage());
        
        using var vfs = VirtualFileSystem.CreateInstance(keySet, true);
        var library = new ApplicationLibrary(vfs, IntegrityCheckLevel.ErrorOnInvalid);

        return library.GetApplicationFromNsp(pfs, file.FullName);
    }
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
