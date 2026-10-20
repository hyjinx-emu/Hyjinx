using LibHac.Tools.FsSystem.NcaUtils;
using System;
using NUnit.Framework;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;

namespace LibHac.Tests.Tools.FsSystem.NcaUtils;

public class NcaFsHeaderTests
{
    [Test]
    public void ReturnsTrueWhenPatchSectionIsNotZeroes()
    {
        Memory<byte> bytes = new byte[HeaderSize];
        "NCA3"u8.CopyTo(bytes.Span[0x200..]);
        GenerateSpan(FsHeaderSize).CopyTo(bytes[FsHeadersOffset..]);

        var header = new NcaHeader(bytes);

        var target = header.GetFsHeader(0);
        Assert.That(target.IsPatchSection(), Is.True);
        Assert.That(target.GetPatchInfo(), Is.Not.Null);
    }

    [Test]
    public void ReturnsFalseWhenPatchSectionIsZeroes()
    {
        Memory<byte> bytes = new byte[HeaderSize];
        "NCA3"u8.CopyTo(bytes.Span[0x200..]);

        var header = new NcaHeader(bytes);

        var target = header.GetFsHeader(0);
        Assert.That(target.IsPatchSection(), Is.False);
        Assert.That(target.GetPatchInfo(), Is.Null);
    }

    private static Memory<byte> GenerateSpan(int length)
    {
        static void Fill(Span<byte> span)
        {
            byte b = 0;
            var index = 0;

            while (index < span.Length)
            {
                span[index] = b;
                b++;

                index++;
            }
        }

        Memory<byte> bytes = new byte[length];
        Fill(bytes.Span);

        return bytes;
    }
}