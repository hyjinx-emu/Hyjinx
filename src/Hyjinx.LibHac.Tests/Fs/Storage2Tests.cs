using LibHac.Fs;
using System;
using Xunit;

namespace LibHac.Tests.Fs;

public class Storage2Tests
{
    [Fact]
    public void ThrowsAnExceptionWhenOffsetIsNegative()
    {
        using var target = new TestStorage2();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => target.Read(-1, new byte[1]));
        Assert.Equal("offset", ex.ParamName);
    }

    [Fact]
    public void ReturnsWithoutReadWhenDestinationIsZeroLength()
    {
        Span<byte> bytes = stackalloc byte[0];

        using var target = new TestStorage2();
        var result = target.Read(0, bytes);

        Assert.Equal(Result.Success, result);
    }

    [Fact]
    public void ThrowsAnExceptionWhenReadExceedsCapacityLimits()
    {
        Memory<byte> bytes = new byte[1];

        using var target = new TestStorage2
        {
            Size = 0
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => target.Read(0, bytes.Span));
        Assert.Equal("destination", ex.ParamName);
    }

    [Fact]
    public void ReturnsSuccessWhenDestinationLengthIsZero()
    {
        Memory<byte> bytes = new byte[0];

        using var target = new TestStorage2
        {
            Size = 0
        };

        var result = target.Read(0, bytes.Span);
        Assert.Equal(Result.Success, result);
    }

    [Fact]
    public void Works()
    {
        Span<byte> bytes = stackalloc byte[1];

        using var target = new TestStorage2
        {
            Size = 1
        };

        var result = target.Read(0, bytes);

        Assert.Equal(Result.Success, result);
        Assert.True(target.HasRead);
    }

    [Fact]
    public void DisposeWorks()
    {
        using var target = new TestStorage2();
        target.Dispose();

        Assert.True(target.IsDisposed);
    }

    [Fact]
    public void ReturnsNotImplemented()
    {
        using var target = new TestStorage2();

        Assert.Equal(ResultFs.NotImplemented.Log(), target.Flush());
        Assert.Equal(ResultFs.NotImplemented.Log(), target.Write(0, new byte[0]));
        Assert.Equal(ResultFs.NotImplemented.Log(), target.SetSize(1));
        Assert.Equal(ResultFs.NotImplemented.Log(), target.OperateRange(OperationId.QueryRange, 0, 0));
        Assert.Equal(ResultFs.NotImplemented.Log(), target.OperateRange(new byte[0], OperationId.QueryRange, 0, 0, new byte[0]));
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