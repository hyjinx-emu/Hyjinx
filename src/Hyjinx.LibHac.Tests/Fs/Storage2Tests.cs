using LibHac.Fs;
using System;
using NUnit.Framework;

namespace LibHac.Tests.Fs;

public class Storage2Tests
{
    [Test]
    public void ThrowsAnExceptionWhenOffsetIsNegative()
    {
        using var target = new TestStorage2();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => target.Read(-1, new byte[1]));
        Assert.That(ex.ParamName, Is.EqualTo("offset"));
    }

    [Test]
    public void ReturnsWithoutReadWhenDestinationIsZeroLength()
    {
        Span<byte> bytes = stackalloc byte[0];

        using var target = new TestStorage2();
        var result = target.Read(0, bytes);

        Assert.That(result, Is.EqualTo(Result.Success));
    }

    [Test]
    public void ThrowsAnExceptionWhenReadExceedsCapacityLimits()
    {
        Memory<byte> bytes = new byte[1];

        using var target = new TestStorage2
        {
            Size = 0
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => target.Read(0, bytes.Span));
        Assert.That(ex.ParamName, Is.EqualTo("destination"));
    }

    [Test]
    public void ReturnsSuccessWhenDestinationLengthIsZero()
    {
        Memory<byte> bytes = new byte[0];

        using var target = new TestStorage2
        {
            Size = 0
        };

        var result = target.Read(0, bytes.Span);
        Assert.That(result, Is.EqualTo(Result.Success));
    }

    [Test]
    public void Works()
    {
        Span<byte> bytes = stackalloc byte[1];

        using var target = new TestStorage2
        {
            Size = 1
        };

        var result = target.Read(0, bytes);

        Assert.That(result, Is.EqualTo(Result.Success));
        Assert.That(target.HasRead, Is.True);
    }

    [Test]
    public void DisposeWorks()
    {
        using var target = new TestStorage2();
        target.Dispose();

        Assert.That(target.IsDisposed, Is.True);
    }

    [Test]
    public void ReturnsNotImplemented()
    {
        using var target = new TestStorage2();

        Assert.That(target.Flush(), Is.EqualTo(ResultFs.NotImplemented.Log()));
        Assert.That(target.Write(0, new byte[0]), Is.EqualTo(ResultFs.NotImplemented.Log()));
        Assert.That(target.SetSize(1), Is.EqualTo(ResultFs.NotImplemented.Log()));
        Assert.That(target.OperateRange(OperationId.QueryRange, 0, 0), Is.EqualTo(ResultFs.NotImplemented.Log()));
        Assert.That(target.OperateRange(new byte[0], OperationId.QueryRange, 0, 0, new byte[0]), Is.EqualTo(ResultFs.NotImplemented.Log()));
    }

    private class TestStorage2 : Storage2
    {
        public bool IsDisposed { get; private set; }
        public bool HasRead { get; private set; }
        public long Size { get; set; }

        public override Result GetSize(out long size)
        {
            size = Size;
            return Result.Success;
        }

        protected override void ReadCore(long offset, Span<byte> buffer)
        {
            HasRead = true;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}