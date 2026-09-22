using LibHac.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using System;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem.NcaUtils;

public class NcaFsPatchInfoTests
{
    [Fact]
    public void Works()
    {
        Memory<byte> rawBytes = new byte[] { 0, 192, 132, 160, 0, 0, 0, 0, 0, 128, 2, 0, 0, 0, 0, 0, 66, 75, 84, 82, 1, 0, 0, 0, 99, 28, 0, 0, 0, 0, 0, 0, 0, 64, 135, 160, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 66, 75, 84, 82, 1, 0, 0, 0, 72, 2, 0, 0, 0, 0, 0, 0 };

        var target = new NcaFsPatchInfo(rawBytes);

        Assert.Equal(2693054464, target.RelocationTreeOffset);
        Assert.Equal(163840, target.RelocationTreeSize);
        Assert.Equal(2693218304, target.EncryptionTreeOffset);
        Assert.Equal(32768, target.EncryptionTreeSize);

        scoped ref var relocationTreeHeader = ref target.GetRelocationTreeHeader();
        Assert.Equal(7267, relocationTreeHeader.EntryCount);
        Assert.Equal(1381256002u, relocationTreeHeader.HeaderSignature);
        Assert.Equal(0, relocationTreeHeader.Reserved);
        Assert.Equal(1u, relocationTreeHeader.Version);

        scoped ref var encryptionTreeHeader = ref target.GetEncryptionTreeHeader();
        Assert.Equal(584, encryptionTreeHeader.EntryCount);
        Assert.Equal(1381256002u, encryptionTreeHeader.HeaderSignature);
        Assert.Equal(0, encryptionTreeHeader.Reserved);
        Assert.Equal(1u, encryptionTreeHeader.Version);
    }
}