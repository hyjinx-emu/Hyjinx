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

        Assert.Throws<ArgumentException>(() => _ = StreamStorage2.Create(stream.Object));

        stream.Setup(o => o.CanRead).Returns(true);
        stream.Setup(o => o.CanSeek).Returns(false);

        Assert.Throws<ArgumentException>(() => _ = StreamStorage2.Create(stream.Object));
    }

    [Fact]
    public void DoesNotDisposeTheStreamByDefault()
    {
        Mock<Stream> stream = new();
        stream.Setup(o => o.CanRead).Returns(true);
        stream.Setup(o => o.CanSeek).Returns(true);
        stream.Setup(o => o.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable(Times.Never);

        var target = StreamStorage2.Create(stream.Object);
        target.Dispose();

        stream.Verify();
    }

    [Fact]
    public void ReadsTheDataFromBeginning()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = StreamStorage2.Create(ms);

        var buffer = new Memory<byte>(new byte[10]);
        storage.Read(0, buffer.Span);
        
        Assert.Equal(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        ], buffer.ToArray());
    }

    [Fact]
    public void ReadsTheDataFromMiddle()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = StreamStorage2.Create(ms);

        var buffer = new Memory<byte>(new byte[10]);
        storage.Read(5, buffer.Span[..5]);
        
        Assert.Equal(
        [
            5, 6, 7, 8, 9, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }

    [Fact]
    public void ReadsTheDataFromEnd()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = StreamStorage2.Create(ms);

        var buffer = new Memory<byte>(new byte[10]);
        storage.Read(9, buffer.Span[..1]);
        
        Assert.Equal(
        [
            9, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }
}