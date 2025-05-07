using Hyjinx.HLE.FileSystem;
using Hyjinx.UI.App.Common;
using LibHac.Common.Keys;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.IO;

namespace LibHac.Tests.Tools;

public class XciTests : GameDecrypterTests
{
    protected override string TargetFileName => "New Super Mario Bros. U Deluxe [0100EA80032EA000][v0] [NKA][C][T].xci";

    protected override string TitleName => "New スーパーマリオブラザーズ U デラックス";
    
    protected override string TargetFileExtension => "xci";

    protected override void DoDecrypt(FileInfo inFile, FileInfo destination)
    {
        var keySet = CreateEncryptedKeySet();
        
        using var inStream = inFile.OpenRead();
        using var outStream = destination.OpenWrite();
        
        var decrypter = new XciDecrypter(keySet);
        decrypter.Decrypt(inStream, outStream);

        outStream.Flush();
    }
    
    protected override ApplicationData ReadApplicationData(KeySet keySet, FileInfo file)
    {
        using var fs = file.OpenRead();
        var xci = new Xci(keySet, fs.AsStorage());
        
        using var vfs = VirtualFileSystem.CreateInstance(keySet, true);
        var library = new ApplicationLibrary(vfs, IntegrityCheckLevel.ErrorOnInvalid);

        var partition = xci.OpenPartition(XciPartitionType.Secure);

        var applications = library.GetApplicationsFromPfs(partition, file.FullName);
        return applications[0];
    }
}
