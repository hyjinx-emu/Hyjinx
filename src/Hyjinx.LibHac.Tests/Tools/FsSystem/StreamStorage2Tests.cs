using LibHac.Tools.FsSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem;

public class StreamStorage2Tests
{
    [Fact]
    public void ConstructorGuards()
    {
        Mock<Stream> stream = new();
        stream.Setup(o => o.CanRead).Returns(false);

        Assert.Throws<ArgumentException>(() => _ = new StreamStorage2(stream.Object));
        
        stream.Setup(o => o.CanRead).Returns(true);
        stream.Setup(o => o.CanSeek).Returns(false);
        
        Assert.Throws<ArgumentException>(() => _ = new StreamStorage2(stream.Object));
    }
    
    [Fact]
    public async Task DoesNotDisposeTheStreamByDefault()
    {
        Mock<Stream> stream = new();
        stream.Setup(o => o.CanRead).Returns(true);
        stream.Setup(o => o.CanSeek).Returns(true);
        stream.Setup(o => o.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable(Times.Never);

        var target = new StreamStorage2(stream.Object);
        await target.DisposeAsync();

        stream.Verify();
    }
    
    [Fact]
    public async Task DisposesTheStream()
    {
        Mock<Stream> stream = new();
        stream.Setup(o => o.CanRead).Returns(true);
        stream.Setup(o => o.CanSeek).Returns(true);
        stream.Setup(o => o.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

        var target = new StreamStorage2(stream.Object, false);
        await target.DisposeAsync();

        stream.Verify();
    }

    [Fact]
    public async Task ReadsTheDataFromBeginning()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = new StreamStorage2(ms);
        
        var buffer = new Memory<byte>(new byte[10]);
        var result = await storage.ReadAsync(buffer);
        
        Assert.Equal(10, result);
        Assert.Equal(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        ], buffer.ToArray());
    }
    
    [Fact]
    public async Task ReadsTheDataFromMiddle()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = new StreamStorage2(ms);
        storage.Seek(5, SeekOrigin.Begin);
        
        var buffer = new Memory<byte>(new byte[10]);
        var result = await storage.ReadAsync(buffer);
        
        Assert.Equal(5, result);
        Assert.Equal(
        [
            5, 6, 7, 8, 9, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }
    
    [Fact]
    public async Task ReadsTheDataFromEnd()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = new StreamStorage2(ms);
        storage.Seek(9, SeekOrigin.Begin);
        
        var buffer = new Memory<byte>(new byte[10]);
        var result = await storage.ReadAsync(buffer);
        
        Assert.Equal(1, result);
        Assert.Equal(
        [
            9, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }

    [Fact]
    public void SeekUpdatesUnderlyingStreamPosition()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        var target = new StreamStorage2(ms);
        
        var position = target.Seek(1, SeekOrigin.Begin);
        Assert.Equal(1, ms.Position);
        Assert.Equal(position, target.Position);
        
        position = target.Seek(3, SeekOrigin.Current);
        Assert.Equal(4, ms.Position);
        Assert.Equal(position, target.Position);
        
        position = target.Seek(-3, SeekOrigin.Current);
        Assert.Equal(1, ms.Position);
        Assert.Equal(position, target.Position);
        
        position = target.Seek(-2, SeekOrigin.End);
        Assert.Equal(8, ms.Position);
        Assert.Equal(position, target.Position);
    }
    
    [Fact]
    public void PositionMatchesUnderlyingStreamPosition()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var target = new StreamStorage2(ms);
        ms.Seek(1, SeekOrigin.Begin);
        Assert.Equal(1, target.Position);
        
        ms.Seek(3, SeekOrigin.Current);
        Assert.Equal(4, target.Position);
        
        ms.Seek(0, SeekOrigin.End);
        Assert.Equal(10, target.Position);
    }
}