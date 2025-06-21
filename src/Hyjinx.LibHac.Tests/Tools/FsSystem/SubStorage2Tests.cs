using LibHac.Tools.FsSystem;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem;

public class SubStorage2Tests
{
    [Fact]
    public void ConstructorGuards()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = new MemoryStream();
            _ = new SubStorage2(new StreamStorage2(ms), 0, 1);
        });
    }

    [Fact]
    public async Task DisposesTheBaseStorage()
    {
        Mock<IAsyncStorage> baseStorage = new();
        
        baseStorage.Setup(o => o.Position).Returns(0);
        baseStorage.Setup(o => o.Length).Returns(1);
        baseStorage.Setup(o => o.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

        var target = new SubStorage2(baseStorage.Object, 0, 1);
        await target.DisposeAsync();

        baseStorage.Verify();
    }

    [Fact]
    public async Task CannotReadPastTheEnd()
    {
        await using var ms = new MemoryStream(
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
        
        await using var storage = new SubStorage2(new StreamStorage2(ms), 0, 1);

        var buffer = new byte[16];
        var result = await storage.ReadAsync(buffer, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Equal(
        [
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    } 
    
    [Fact]
    public async Task ThrowsAnExceptionWhenSeekingBeforeTheStartPosition()
    {
        await using var ms = new MemoryStream(
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);

        var offset = 1;
        ms.Seek(offset, SeekOrigin.Begin);
        
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await using var storage = new SubStorage2(new StreamStorage2(ms), offset, ms.Length - offset);
            storage.Seek(-1, SeekOrigin.Begin);
        });
        
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await using var storage = new SubStorage2(new StreamStorage2(ms), offset, ms.Length - offset);
            storage.Seek(int.MinValue, SeekOrigin.Current);
        });
        
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await using var storage = new SubStorage2(new StreamStorage2(ms), offset, ms.Length - offset);
            storage.Seek(int.MinValue, SeekOrigin.End);
        });
    }
    
    [Fact]
    public async Task ThrowsAnExceptionWhenSeekingAfterTheEnd()
    {
        await using var ms = new MemoryStream(
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
        
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await using var storage = new SubStorage2(new StreamStorage2(ms), 0, ms.Length);
            storage.Seek(ms.Length + 1, SeekOrigin.Begin);
        });

        // Move to the end of the stream.
        ms.Seek(-1, SeekOrigin.End);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await using var storage = new SubStorage2(new StreamStorage2(ms),0, 1);
            storage.Seek(2, SeekOrigin.Current);
        });
        
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await using var storage = new SubStorage2(new StreamStorage2(ms),0, 1);
            storage.Seek(1, SeekOrigin.End);
        });
    }
    
    [Fact]
    public async Task SeeksTheStreamFromTheBeginningAsExpected()
    {
        await using var ms = new MemoryStream(
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);

        byte[] buffer = new byte[1];
        await using var storage = new SubStorage2(new StreamStorage2(ms),0, ms.Length);

        // Reposition to the 4th index, and read the value.
        var pos = storage.Seek(4, SeekOrigin.Begin);
        Assert.Equal(4, pos);
        
        var result = await storage.ReadAsync(buffer, CancellationToken.None);
        
        Assert.Equal(1, result);
        Assert.Equal([5], buffer);

        // Reposition back to the 4th index, and read the value again.
        pos = storage.Seek(-1, SeekOrigin.Current);
        Assert.Equal(4, pos);
        
        result = await storage.ReadAsync(buffer, CancellationToken.None);
        
        Assert.Equal(1, result);
        Assert.Equal([5], buffer);
    }
    
    [Fact]
    public async Task SeeksTheStreamFromTheEndAsExpected()
    {
        using var ms = new MemoryStream(
        [
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);

        await using var storage = new SubStorage2(new StreamStorage2(ms),0, ms.Length);

        // Read the first value.
        byte[] buffer = new byte[1];
        var result = await storage.ReadAsync(buffer, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Equal([1], buffer);
    }

    [Fact]
    public async Task ReadAtEndOfStreamReturnsZero()
    {
        await using var ms = new MemoryStream([1]);

        await using var storage = new SubStorage2(new StreamStorage2(ms),0, 1);
        storage.Seek(1, SeekOrigin.Begin);

        var buffer = new byte[16];
        var result = await storage.ReadAsync(buffer, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Equal(
        [
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
        ], buffer);
    }
}