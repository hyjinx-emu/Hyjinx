using LibHac.Fs;
using System;
using Xunit;

namespace LibHac.Tests.Fs;

public class MemoryStorage2Tests
{
    [Fact]
    public void ReadsTheData()
    {
        byte[] data =
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ];

        using var target = MemoryStorage2.Create(data);

        var buffer = new Memory<byte>(new byte[32]);
        target.Read(0, buffer.Span);
        
        Assert.Equal([
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ], buffer.ToArray());
    }
}