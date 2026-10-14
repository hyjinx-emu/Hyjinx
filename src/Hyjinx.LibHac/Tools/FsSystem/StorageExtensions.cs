using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using System;
using System.IO;

namespace LibHac.Tools.FsSystem;

public static partial class StorageExtensions
{
    public static IStorage Slice(this IStorage storage, long start)
    {
        storage.GetSize(out long length).ThrowIfFailure();

        if (length == -1)
        {
            return storage.Slice(start, length);
        }

        return storage.Slice(start, length - start);
    }

    public static IStorage Slice(this IStorage storage, long start, long length)
    {
        return storage.Slice(start, length, true);
    }

    public static IStorage Slice(this IStorage storage, long start, long length, bool leaveOpen)
    {
        if (!leaveOpen)
        {
            return new SubStorage(storage, start, length);
        }

        using (var sharedStorage = new SharedRef<IStorage>(storage))
        {
            return new SubStorage(in sharedStorage, start, length);
        }
    }

    public static Stream AsStream(this IStorage storage) => new StorageStream(storage, FileAccess.ReadWrite, true);

    public static IFile AsFile(this IStorage storage, OpenMode mode) => new StorageFile(storage, mode);

    public static void CopyTo(this IStorage input, IStorage output, IProgressReport progress = null, int bufferSize = 81920)
    {
        input.GetSize(out long inputSize).ThrowIfFailure();
        output.GetSize(out long outputSize).ThrowIfFailure();

        long remaining = Math.Min(inputSize, outputSize);
        if (remaining < 0)
            throw new ArgumentException("Storage must have an explicit length");
        progress?.SetTotal(remaining);

        long pos = 0;

        using var buffer = new RentedArray<byte>(bufferSize);
        int rentedBufferSize = buffer.Array.Length;

        while (remaining > 0)
        {
            int toCopy = (int)Math.Min(rentedBufferSize, remaining);
            Span<byte> buf = buffer.Array.AsSpan(0, toCopy);
            input.Read(pos, buf);
            output.Write(pos, buf);

            remaining -= toCopy;
            pos += toCopy;

            progress?.ReportAdd(toCopy);
        }

        progress?.SetTotal(0);
    }

    public static void CopyToStream(this IStorage input, Stream output, long length, IProgressReport progress = null, int bufferSize = 0x8000)
    {
        long remaining = length;
        long inOffset = 0;

        using var buffer = new RentedArray<byte>(bufferSize);
        int rentedBufferSize = buffer.Array.Length;

        progress?.SetTotal(length);

        while (remaining > 0)
        {
            int toWrite = (int)Math.Min(rentedBufferSize, remaining);
            input.Read(inOffset, buffer.Array.AsSpan(0, toWrite));

            output.Write(buffer.Array, 0, toWrite);
            remaining -= toWrite;
            inOffset += toWrite;
            progress?.ReportAdd(toWrite);
        }
    }

    public static IStorage AsStorage(this Stream stream)
    {
        if (stream == null)
            return null;
        return new StreamStorage(stream, true);
    }

    public static IStorage AsStorage(this Stream stream, bool keepOpen)
    {
        if (stream == null)
            return null;
        return new StreamStorage(stream, keepOpen);
    }
}