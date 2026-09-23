using LibHac.Tools.FsSystem.NcaUtils;
using System;
using static LibHac.Tools.FsSystem.NcaUtils.NativeTypes;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem.NcaUtils;

public class NcaFsHeaderTests
{
    [Fact]
    public void ReturnsTrueWhenPatchSectionIsNotZeroes()
    {
        Memory<byte> bytes = new byte[HeaderSize];
        "NCA3"u8.CopyTo(bytes.Span[0x200..]);
        GenerateSpan(FsHeaderSize).CopyTo(bytes[FsHeadersOffset..]);

        var header = new NcaHeader(bytes);

        var target = header.GetFsHeader(0);
        Assert.True(target.IsPatchSection());
        Assert.NotNull(target.GetPatchInfo());
    }

    [Fact]
    public void ReturnsFalseWhenPatchSectionIsZeroes()
    {
        Memory<byte> bytes = new byte[HeaderSize];
        "NCA3"u8.CopyTo(bytes.Span[0x200..]);

        var header = new NcaHeader(bytes);

        var target = header.GetFsHeader(0);
        Assert.False(target.IsPatchSection());
        Assert.Null(target.GetPatchInfo());
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