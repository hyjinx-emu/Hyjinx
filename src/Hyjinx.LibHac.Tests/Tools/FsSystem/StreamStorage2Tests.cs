using LibHac.Tools.FsSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LibHac.Tests.Tools.FsSystem;

public class StreamStorage2Tests
{
    [Test]
    public void ConstructorGuards()
    {
        Mock<Stream> stream = new();
        stream.Setup(o => o.CanRead).Returns(false);

        Assert.Throws<ArgumentException>(() => _ = StreamStorage2.Create(stream.Object));

        stream.Setup(o => o.CanRead).Returns(true);
        stream.Setup(o => o.CanSeek).Returns(false);

        Assert.Throws<ArgumentException>(() => _ = StreamStorage2.Create(stream.Object));
    }

    [Test]
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

    [Test]
    public void ReadsTheDataFromBeginning()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = StreamStorage2.Create(ms);

        var buffer = new Memory<byte>(new byte[10]);
        storage.Read(0, buffer.Span);

        Assert.That(buffer.ToArray(), Is.EqualTo(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        ]));
    }

    [Test]
    public void ReadsTheDataFromMiddle()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = StreamStorage2.Create(ms);

        var buffer = new Memory<byte>(new byte[10]);
        storage.Read(5, buffer.Span[..5]);

        Assert.That(buffer.ToArray(), Is.EqualTo(
        [
            5, 6, 7, 8, 9, 0, 0, 0, 0, 0
        ]));
    }

    [Test]
    public void ReadsTheDataFromEnd()
    {
        using var ms = new MemoryStream([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        var storage = StreamStorage2.Create(ms);

        var buffer = new Memory<byte>(new byte[10]);
        storage.Read(9, buffer.Span[..1]);

        Assert.That(buffer.ToArray(), Is.EqualTo(
        [
            9, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ]));
    }
}