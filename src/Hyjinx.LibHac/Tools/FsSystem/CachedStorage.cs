using LibHac.Fs;
using System;
using System.Collections.Generic;

namespace LibHac.Tools.FsSystem;

public class CachedStorage : IStorage
{
    private IStorage BaseStorage { get; }
    private int BlockSize { get; }
    private long Length { get; set; }
    private bool LeaveOpen { get; }

    private LinkedList<CacheBlock> Blocks { get; } = new LinkedList<CacheBlock>();
    private Dictionary<long, LinkedListNode<CacheBlock>> BlockDict { get; } = new Dictionary<long, LinkedListNode<CacheBlock>>();

    public CachedStorage(IStorage baseStorage, int blockSize, int cacheSize, bool leaveOpen)
    {
        BaseStorage = baseStorage;
        BlockSize = blockSize;
        LeaveOpen = leaveOpen;

        BaseStorage.GetSize(out long baseSize).ThrowIfFailure();
        Length = baseSize;

        for (int i = 0; i < cacheSize; i++)
        {
            var block = new CacheBlock { Buffer = new byte[blockSize], Index = -1 };
            Blocks.AddLast(block);
        }
    }

    public CachedStorage(SectorStorage baseStorage, int cacheSize, bool leaveOpen)
        : this(baseStorage, baseStorage.SectorSize, cacheSize, leaveOpen) { }

    public override Result Read(long offset, Span<byte> destination)
    {
        long remaining = destination.Length;
        long inOffset = offset;
        int outOffset = 0;

        Result res = CheckAccessRange(offset, destination.Length, Length);
        if (res.IsFailure())
            return res.Miss();

        lock (Blocks)
        {
            while (remaining > 0)
            {
                long blockIndex = inOffset / BlockSize;
                int blockPos = (int)(inOffset % BlockSize);
                CacheBlock block = GetBlock(blockIndex);

                int bytesToRead = (int)Math.Min(remaining, BlockSize - blockPos);

                block.Buffer.AsSpan(blockPos, bytesToRead).CopyTo(destination.Slice(outOffset));

                outOffset += bytesToRead;
                inOffset += bytesToRead;
                remaining -= bytesToRead;
            }
        }

        return Result.Success;
    }

    public override Result Write(long offset, ReadOnlySpan<byte> source)
    {
        long remaining = source.Length;
        long inOffset = offset;
        int outOffset = 0;

        Result res = CheckAccessRange(offset, source.Length, Length);
        if (res.IsFailure())
            return res.Miss();

        lock (Blocks)
        {
            while (remaining > 0)
            {
                long blockIndex = inOffset / BlockSize;
                int blockPos = (int)(inOffset % BlockSize);
                CacheBlock block = GetBlock(blockIndex);

                int bytesToWrite = (int)Math.Min(remaining, BlockSize - blockPos);

                source.Slice(outOffset, bytesToWrite).CopyTo(block.Buffer.AsSpan(blockPos));

                block.Dirty = true;

                outOffset += bytesToWrite;
                inOffset += bytesToWrite;
                remaining -= bytesToWrite;
            }
        }

        return Result.Success;
    }

    public override Result Flush()
    {
        lock (Blocks)
        {
            foreach (CacheBlock cacheItem in Blocks)
            {
                FlushBlock(cacheItem);
            }
        }

        return BaseStorage.Flush();
    }

    public override Result GetSize(out long size)
    {
        size = Length;
        return Result.Success;
    }

    public override Result SetSize(long size)
    {
        Result res = BaseStorage.SetSize(size);
        if (res.IsFailure())
            return res.Miss();

        res = BaseStorage.GetSize(out long newSize);
        if (res.IsFailure())
            return res.Miss();

        Length = newSize;

        return Result.Success;
    }

    public override Result OperateRange(Span<byte> outBuffer, OperationId operationId, long offset, long size,
        ReadOnlySpan<byte> inBuffer)
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        if (!LeaveOpen)
        {
            BaseStorage?.Dispose();
        }

        base.Dispose();
    }

    private CacheBlock GetBlock(long blockIndex)
    {
        if (BlockDict.TryGetValue(blockIndex, out LinkedListNode<CacheBlock> node))
        {
            if (Blocks.First != node)
            {
                Blocks.Remove(node);
                Blocks.AddFirst(node);
            }

            return node!.Value;
        }

        // An inactive node shouldn't be null, but we'll fix it if it is anyway
        node = Blocks.Last ??
               new LinkedListNode<CacheBlock>(new CacheBlock { Buffer = new byte[BlockSize], Index = -1 });

        FlushBlock(node.Value);

        CacheBlock block = node.Value;
        Blocks.RemoveLast();

        if (block.Index != -1)
        {
            BlockDict.Remove(block.Index);
        }

        FlushBlock(block);
        ReadBlock(block, blockIndex);

        Blocks.AddFirst(node);
        BlockDict.Add(blockIndex, node);

        return block;
    }

    private void ReadBlock(CacheBlock block, long index)
    {
        long offset = index * BlockSize;
        int length = BlockSize;

        if (Length != -1)
        {
            length = (int)Math.Min(Length - offset, length);
        }

        BaseStorage.Read(offset, block.Buffer.AsSpan(0, length)).ThrowIfFailure();
        block.Length = length;
        block.Index = index;
        block.Dirty = false;
    }

    private void FlushBlock(CacheBlock block)
    {
        if (!block.Dirty)
            return;

        long offset = block.Index * BlockSize;
        BaseStorage.Write(offset, block.Buffer.AsSpan(0, block.Length)).ThrowIfFailure();
        block.Dirty = false;
    }

    private class CacheBlock
    {
        public long Index { get; set; }
        public byte[] Buffer { get; set; }
        public int Length { get; set; }
        public bool Dirty { get; set; }
    }
}