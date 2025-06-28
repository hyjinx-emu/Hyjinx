using LibHac.FsSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace LibHac.Tests.FsSystem;

public class MemoryStorage2Tests
{
    [Fact]
    public async Task SeeksTheStorage()
    {
        byte[] data =
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ];

        await using var target = MemoryStorage2.Create(data);
        
        target.Seek(1, SeekOrigin.Begin);
        
        var buffer = new Memory<byte>(new byte[32]);
        var result = await target.ReadAsync(buffer);
        
        Assert.Equal(31, result);
        Assert.Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 
            18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 0
        ], buffer.ToArray());
    }
    
    [Fact]
    public async Task ReadsTheData()
    {
        byte[] data =
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ];

        await using var target = MemoryStorage2.Create(data);

        var buffer = new Memory<byte>(new byte[32]);
        var result = await target.ReadAsync(buffer);
        
        Assert.Equal(32, result);
        Assert.Equal([
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ], buffer.ToArray());
    }
}