using System.IO;

namespace LibHac.Tests.Tools;

public class XciTests : GameDecrypterTests
{
    protected override string TargetFileName => "";

    protected override void DoDecrypt(FileInfo inFile, FileInfo destination)
    {
        throw new System.NotImplementedException();
    }
}
