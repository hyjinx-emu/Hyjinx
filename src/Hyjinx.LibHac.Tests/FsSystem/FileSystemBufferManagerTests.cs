using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Mem;
using NUnit.Framework;
using CacheHandle = System.UInt64;

namespace LibHac.Tests.FsSystem;

public class FileSystemBufferManagerTests
{
    private static void AssertSuccess(Result result)
    {
        Assert.That(result.IsSuccess(), Is.True, result.ToString());
    }

    private FileSystemBufferManager CreateManager(int size, int blockSize = 0x4000, int maxCacheCount = 16)
    {
        int orderMax = FileSystemBuddyHeap.QueryOrderMax((nuint)size, (nuint)blockSize);
        nuint workBufferSize = FileSystemBuddyHeap.QueryWorkBufferSize(orderMax);
        byte[] workBuffer = new byte[workBufferSize];
        byte[] heapBuffer = new byte[size];

        var bufferManager = new FileSystemBufferManager();
        AssertSuccess(bufferManager.Initialize(maxCacheCount, heapBuffer, blockSize, workBuffer));
        return bufferManager;
    }

    [Test]
    public void AllocateBuffer_NoFreeSpace_ReturnsNull()
    {
        FileSystemBufferManager manager = CreateManager(0x20000);
        Buffer buffer1 = manager.AllocateBuffer(0x10000);
        Buffer buffer2 = manager.AllocateBuffer(0x10000);
        Buffer buffer3 = manager.AllocateBuffer(0x4000);

        Assert.That(buffer1.IsNull, Is.False);
        Assert.That(buffer2.IsNull, Is.False);
        Assert.That(buffer3.IsNull, Is.True);
    }

    [Test]
    public void AcquireCache_EntryNotEvicted_ReturnsEntry()
    {
        FileSystemBufferManager manager = CreateManager(0x20000);
        Buffer buffer1 = manager.AllocateBuffer(0x10000);

        CacheHandle handle = manager.RegisterCache(buffer1, new IBufferManager.BufferAttribute());

        manager.AllocateBuffer(0x10000);
        Buffer buffer3 = manager.AcquireCache(handle);

        Assert.That(buffer3, Is.EqualTo(buffer1));
    }

    [Test]
    public void AcquireCache_EntryEvicted_ReturnsNull()
    {
        FileSystemBufferManager manager = CreateManager(0x20000);
        Buffer buffer1 = manager.AllocateBuffer(0x10000);

        CacheHandle handle = manager.RegisterCache(buffer1, new IBufferManager.BufferAttribute());

        manager.AllocateBuffer(0x20000);
        Buffer buffer3 = manager.AcquireCache(handle);

        Assert.That(buffer3.IsNull, Is.True);
    }

    [Test]
    public void AcquireCache_MultipleEntriesEvicted_OldestAreEvicted()
    {
        FileSystemBufferManager manager = CreateManager(0x20000);
        Buffer buffer1 = manager.AllocateBuffer(0x8000);
        Buffer buffer2 = manager.AllocateBuffer(0x8000);
        Buffer buffer3 = manager.AllocateBuffer(0x8000);
        Buffer buffer4 = manager.AllocateBuffer(0x8000);

        CacheHandle handle1 = manager.RegisterCache(buffer1, new IBufferManager.BufferAttribute());
        CacheHandle handle2 = manager.RegisterCache(buffer2, new IBufferManager.BufferAttribute());
        CacheHandle handle3 = manager.RegisterCache(buffer3, new IBufferManager.BufferAttribute());
        CacheHandle handle4 = manager.RegisterCache(buffer4, new IBufferManager.BufferAttribute());

        manager.AllocateBuffer(0x10000);

        Buffer buffer1B = manager.AcquireCache(handle1);
        Buffer buffer2B = manager.AcquireCache(handle2);
        Buffer buffer3B = manager.AcquireCache(handle3);
        Buffer buffer4B = manager.AcquireCache(handle4);

        Assert.That(buffer1B.IsNull, Is.True);
        Assert.That(buffer2B.IsNull, Is.True);
        Assert.That(buffer3B, Is.EqualTo(buffer3));
        Assert.That(buffer4B, Is.EqualTo(buffer4));
    }
}