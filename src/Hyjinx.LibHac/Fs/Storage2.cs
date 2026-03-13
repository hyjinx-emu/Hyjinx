using System;

namespace LibHac.Fs;

/// <summary>
/// A base <see cref="IStorage"/> implementation. This class must be inherited.
/// </summary>
public abstract class Storage2 : IStorage
{
    ~Storage2()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // This method intentionally left blank.
    }

    public Result Read(long offset, Span<byte> destination)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The value cannot be less than zero.");
        }

        GetSize(out var size).ThrowIfFailure();
        if (offset + destination.Length > size)
        {
            throw new ArgumentOutOfRangeException(nameof(destination), "The size of the destination exceeds the remaining storage capacity.");
        }

        ReadCore(offset, destination);
        return Result.Success;
    }

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="offset">The zero-based offset which to seek.</param>
    /// <param name="buffer">The buffer which should receive the data.</param>
    protected abstract void ReadCore(long offset, Span<byte> buffer);

    public Result Write(long offset, ReadOnlySpan<byte> source)
    {
        return ResultFs.NotImplemented.Log();
    }

    public Result Flush()
    {
        return ResultFs.NotImplemented.Log();
    }

    public abstract Result GetSize(out long size);

    public Result SetSize(long size)
    {
        return ResultFs.NotImplemented.Log();
    }

    public Result OperateRange(Span<byte> outBuffer, OperationId operationId, long offset, long size, ReadOnlySpan<byte> inBuffer)
    {
        return ResultFs.NotImplemented.Log();
    }

    public Result OperateRange(OperationId operationId, long offset, long size)
    {
        return ResultFs.NotImplemented.Log();
    }
}