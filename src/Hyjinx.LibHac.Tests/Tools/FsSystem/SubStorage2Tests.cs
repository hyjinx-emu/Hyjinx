using LibHac.Fs;
using LibHac.Tools.FsSystem;
using System;
using Xunit;

namespace LibHac.Tests.Tools.FsSystem;

public class SubStorage2Tests
{
    [Fact]
    public void ConstructorGuards()
    {
        var baseStorage = MemoryStorage2.Create([]);

        // Throws an exception when offset is negative.
        Assert.Throws<ArgumentException>(() => _ = SubStorage2.Create(baseStorage, -1, 1));

        // Throws an exception when length is negative.
        Assert.Throws<ArgumentException>(() => _ = SubStorage2.Create(baseStorage, 0, -1));

        // Throws an exception when window exceeds the capacity of the base storage.
        Assert.Throws<ArgumentException>(() => _ = SubStorage2.Create(baseStorage, 0, 1));
    }

    /// <summary>
    /// This is typically used by an empty file being present on a file system.
    /// </summary>
    [Fact]
    public void SupportsZeroLengthValues()
    {
        var baseStorage = MemoryStorage2.Create([]);

        var target = SubStorage2.Create(baseStorage, 0, 0);

        Assert.Equal(0, target.Size);
    }

    [Fact]
    public void InitializesCorrectlyWithOffsets()
    {
        using var target = SubStorage2.Create(MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]), 1, 10);

        Assert.Equal(10, target.Size);
    }

    [Fact]
    public void ReadsTheBytesAsExpected()
    {
        var ms = MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
        using var target = SubStorage2.Create(ms, 0, 10);

        Memory<byte> buffer = new byte[32];
        target.Read(0, buffer.Span[..10]);

        Assert.Equal([
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }

    [Fact]
    public void ReadsTheBytesFromOffsetAsExpected()
    {
        var ms = MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
        using var target = SubStorage2.Create(ms, 1, 10);

        Memory<byte> buffer = new byte[32];
        target.Read(0, buffer.Span[..10]);

        Assert.Equal([
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }

    [Fact]
    public void DisposesTheBaseStorage()
    {
        Mock<IStorage2> baseStorage = new();

        baseStorage.Setup(o => o.Size).Returns(1);
        baseStorage.Setup(o => o.Dispose()).Verifiable();

        var target = SubStorage2.Create(baseStorage.Object, 0, 1);
        target.Dispose();

        baseStorage.Verify();
    }

    [Fact]
    public void ReadsUpToTheEnd()
    {
        using var ms = MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);

        using var target = SubStorage2.Create(ms, 0, 2);

        Memory<byte> buffer = new byte[16];
        target.Read(0, buffer.Span[..2]);

        Assert.Equal(
        [
            0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }

    [Fact]
    public void ReadAtEndOfStreamReturnsZero()
    {
        using var ms = MemoryStorage2.Create([1]);

        using var target = SubStorage2.Create(ms, 0, 1);

        Memory<byte> buffer = new byte[16];
        target.Read(1, buffer.Span[..0]);

        Assert.Equal(
        [
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer.ToArray());
    }
}