using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.IO;

namespace LibHac.Tests.Tools;

public class XciTests : GameDecrypterTests
{
    protected override string TargetFileName => "New Super Mario Bros. U Deluxe [0100EA80032EA000][v0] [NKA][C][T].xci";

    protected override void DoDecrypt(FileInfo inFile, FileInfo destination)
    {
        var keySet = CreateEncryptedKeySet();
        
        using var inStream = inFile.OpenRead();

        var xci = new Xci(keySet, inStream.AsStorage());
        using var outStream = destination.OpenWrite();
        
        var decrypter = new XciDecrypter(keySet);
        decrypter.Decrypt(xci, outStream);

        outStream.Flush();
    }
}
