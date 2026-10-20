using LibHac.Tools.FsSystem.NcaUtils;
using System;
using NUnit.Framework;

namespace LibHac.Tests.Tools.FsSystem.NcaUtils;

public class NcaFsPatchInfoTests
{
    [Test]
    public void Works()
    {
        Memory<byte> rawBytes = new byte[] { 0, 192, 132, 160, 0, 0, 0, 0, 0, 128, 2, 0, 0, 0, 0, 0, 66, 75, 84, 82, 1, 0, 0, 0, 99, 28, 0, 0, 0, 0, 0, 0, 0, 64, 135, 160, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 66, 75, 84, 82, 1, 0, 0, 0, 72, 2, 0, 0, 0, 0, 0, 0 };

        var target = new NcaFsPatchInfo(rawBytes);

        Assert.That(target.RelocationTreeOffset, Is.EqualTo(2693054464));
        Assert.That(target.RelocationTreeSize, Is.EqualTo(163840));
        Assert.That(target.EncryptionTreeOffset, Is.EqualTo(2693218304));
        Assert.That(target.EncryptionTreeSize, Is.EqualTo(32768));

        var relocationTreeHeader = target.GetRelocationTreeHeader();
        Assert.That(relocationTreeHeader.EntryCount, Is.EqualTo(7267));
        Assert.That(relocationTreeHeader.HeaderSignature, Is.EqualTo(1381256002u));
        Assert.That(relocationTreeHeader.Reserved, Is.EqualTo(0));
        Assert.That(relocationTreeHeader.Version, Is.EqualTo(1u));

        var encryptionTreeHeader = target.GetEncryptionTreeHeader();
        Assert.That(encryptionTreeHeader.EntryCount, Is.EqualTo(584));
        Assert.That(encryptionTreeHeader.HeaderSignature, Is.EqualTo(1381256002u));
        Assert.That(encryptionTreeHeader.Reserved, Is.EqualTo(0));
        Assert.That(encryptionTreeHeader.Version, Is.EqualTo(1u));
    }
}