#if IS_TPM_BYPASS_ENABLED
#pragma warning disable 0618 // Type or member is obsolete

using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.IO;

namespace LibHac.Tests.Tools;

public class NspTests : GameDecrypterTests
{
    protected override string TargetFileName => "Baba Is You [01002CD00A51C800][v720896][Update].nsp";

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
}

#pragma warning restore 0618 // Type or member is obsolete
#endif
