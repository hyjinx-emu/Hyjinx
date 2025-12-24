using LibHac.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.IO;
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
        
        Assert.Equal(0, target.Length);
    }

    [Fact]
    public void InitializesCorrectlyWithOffsets()
    {
        using var target = SubStorage2.Create(MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]), 1, 10);

        Assert.Equal(0, target.Position);
        Assert.Equal(10, target.Length);
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
        
        byte[] buffer = new byte[32];
        var bytesRead = target.Read(buffer);
        
        Assert.Equal(10, bytesRead);
        Assert.Equal(10, target.Position);
        Assert.Equal([
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
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
        
        byte[] buffer = new byte[32];
        var bytesRead = target.Read(buffer);
        
        Assert.Equal(10, bytesRead);
        Assert.Equal(10, target.Position);
        Assert.Equal([
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    }

    [Fact]
    public void DisposesTheBaseStorage()
    {
        Mock<IStorage2> baseStorage = new();
        
        baseStorage.Setup(o => o.Position).Returns(0);
        baseStorage.Setup(o => o.Length).Returns(1);
        baseStorage.Setup(o => o.Dispose()).Verifiable();
    
        var target = SubStorage2.Create(baseStorage.Object, 0, 1);
        target.Dispose();
    
        baseStorage.Verify();
    }
    
    [Fact]
    public void CannotReadPastTheEnd()
    {
        using var ms = MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
        
        using var target = SubStorage2.Create(ms, 0, 2);
    
        var buffer = new byte[16];
        var bytesRead = target.Read(buffer);
    
        Assert.Equal(2, bytesRead);
        Assert.Equal(
        [
            0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    } 
    
    [Fact]
    public void ThrowsAnExceptionWhenSeekingBeforeTheStartOffsetWithBegin()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = MemoryStorage2.Create(
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            ]);
    
            var offset = 1;
            
            using var target = SubStorage2.Create(ms, offset, ms.Length - offset);
            target.Seek(-1, SeekOrigin.Begin);
        });
    }
    
    [Fact]
    public void ThrowsAnExceptionWhenSeekingBeforeTheStartOffsetWithCurrent()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = MemoryStorage2.Create(
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            ]);
    
            var offset = 1;
            
            using var target = SubStorage2.Create(ms, offset, ms.Length - offset);
            target.Seek(int.MinValue, SeekOrigin.Current);
        });
    }
    
    [Fact]
    public void ThrowsAnExceptionWhenSeekingBeforeTheStartOffsetWithEnd()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = MemoryStorage2.Create(
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            ]);

            var offset = 1;
            
            using var target = SubStorage2.Create(ms, offset, ms.Length - offset);
            target.Seek(int.MinValue, SeekOrigin.End);
        });
    }
    
    [Fact]
    public void ThrowsAnExceptionWhenSeekingAfterTheEndFromBegin()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = MemoryStorage2.Create(
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            ]);
            
            using var target = SubStorage2.Create(ms, 0, ms.Length);
            target.Seek(ms.Length + 1, SeekOrigin.Begin);
        });
    }

    [Fact]
    public void ThrowsAnExceptionWhenSeekingAfterTheEndFromCurrent()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = MemoryStorage2.Create(
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            ]);
            
            using var target = SubStorage2.Create(ms,0, 1);
            target.Seek(2, SeekOrigin.Current);
        });
    }

    [Fact]
    public void ThrowsAnExceptionWhenSeekingAfterTheEndFromEnd()
    {   
        Assert.Throws<ArgumentException>(() =>
        {
            using var ms = MemoryStorage2.Create(
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            ]);
            
            using var target = SubStorage2.Create(ms,0, 1);
            target.Seek(1, SeekOrigin.End);
        });
    }

    [Fact]
    public void SeeksTheStreamFromTheBeginningAsExpected()
    {
        using var ms = MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
    
        var offset = 1;
        var length = 10;
        
        byte[] buffer = new byte[ms.Length];
        using var target = SubStorage2.Create(ms, offset, length);
    
        // Reposition to the 4th index, and read the value.
        var pos = target.Seek(4, SeekOrigin.Begin);
        Assert.Equal(4, pos);
        Assert.Equal(4, target.Position);
        
        var bytesRead = target.Read(buffer);
        
        Assert.Equal(6, bytesRead);
        Assert.Equal(10, target.Position);
        Assert.Equal(
        [
            5, 6, 7, 8, 9, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    
        // Reposition back to the 4th index, and read the value again.
        pos = target.Seek(-1, SeekOrigin.Current);
        Assert.Equal(9, pos);
        
        bytesRead = target.Read(buffer);
        
        Assert.Equal(1, bytesRead);
        Assert.Equal(10, target.Position);
        Assert.Equal(
        [
            10, 6, 7, 8, 9, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    }
    
    [Fact]
    public void SeeksTheStreamFromTheEndAsExpected()
    {
        using var ms = MemoryStorage2.Create(
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
        ]);
    
        using var target = SubStorage2.Create(ms,1, 10);

        var pos = target.Seek(-1, SeekOrigin.End);
        Assert.Equal(9, pos);
        Assert.Equal(9, target.Position);
        
        // Read the first value.
        byte[] buffer = new byte[ms.Length];
        
        var bytesRead = target.Read(buffer);
        Assert.Equal(1, bytesRead);
        Assert.Equal(
        [
            10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
        
        pos = target.Seek(-2, SeekOrigin.Current);
        Assert.Equal(8, pos);
        Assert.Equal(8, target.Position);

        bytesRead = target.Read(buffer);
        Assert.Equal(2, bytesRead);
        Assert.Equal(
        [
            9, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    }
    
    [Fact]
    public void ReadAtEndOfStreamReturnsZero()
    {
        using var ms = MemoryStorage2.Create([1]);

        using var target = SubStorage2.Create(ms, 0, 1);
        target.Seek(1, SeekOrigin.Begin);

        var buffer = new byte[16];
        var result = target.Read(buffer);

        Assert.Equal(0, result);
        Assert.Equal(
        [
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        ], buffer);
    }
}