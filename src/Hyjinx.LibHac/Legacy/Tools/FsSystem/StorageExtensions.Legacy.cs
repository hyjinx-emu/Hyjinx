#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibHac.Tools.FsSystem;

partial class StorageExtensions
{
    public static Stream AsStream(this IStorage storage, FileAccess access) => new StorageStream(storage, access, true);
    public static Stream AsStream(this IStorage storage, FileAccess access, bool keepOpen) => new StorageStream(storage, access, keepOpen);

    public static void Fill(this IStorage input, byte value, IProgressReport progress = null)
    {
        input.GetSize(out long inputSize).ThrowIfFailure();
        input.Fill(value, 0, inputSize, progress);
    }

    public static void Fill(this IStorage input, byte value, long offset, long count, IProgressReport progress = null)
    {
        const int threshold = 0x400;

        if (count > threshold)
        {
            input.FillLarge(value, offset, count, progress);
            return;
        }

        Span<byte> buf = stackalloc byte[(int)count];
        buf.Fill(value);

        input.Write(offset, buf);
    }

    private static void FillLarge(this IStorage input, byte value, long offset, long count, IProgressReport progress = null)
    {
        const int bufferSize = 0x4000;

        long remaining = count;
        if (remaining < 0)
            throw new ArgumentException("Storage must have an explicit length");
        progress?.SetTotal(remaining);

        long pos = offset;

        using var buffer = new RentedArray<byte>(bufferSize);
        int rentedBufferSize = buffer.Array.Length;

        buffer.Array.AsSpan(0, (int)Math.Min(remaining, rentedBufferSize)).Fill(value);

        while (remaining > 0)
        {
            int toFill = (int)Math.Min(rentedBufferSize, remaining);
            Span<byte> buf = buffer.Array.AsSpan(0, toFill);

            input.Write(pos, buf);

            remaining -= toFill;
            pos += toFill;

            progress?.ReportAdd(toFill);
        }

        progress?.SetTotal(0);
    }

    public static void CopyToStream(this IStorage input, Stream output, int bufferSize = 0x8000)
    {
        input.GetSize(out long inputSize).ThrowIfFailure();
        CopyToStream(input, output, inputSize, bufferSize: bufferSize);
    }

    public static IStorage AsStorage(this Stream stream, long start)
    {
        if (stream == null)
            return null;
        return new StreamStorage(stream, true).Slice(start);
    }

    public static IStorage AsStorage(this Stream stream, long start, int length)
    {
        if (stream == null)
            return null;
        return new StreamStorage(stream, true).Slice(start, length);
    }

    public static IStorage AsStorage(this Stream stream, long start, int length, bool keepOpen)
    {
        if (stream == null)
            return null;
        return new StreamStorage(stream, keepOpen).Slice(start, length);
    }

    public static byte[] ToArray(this IStorage storage)
    {
        if (storage == null)
            return new byte[0];

        storage.GetSize(out long storageSize).ThrowIfFailure();

        byte[] arr = new byte[storageSize];
        storage.CopyTo(new MemoryStorage(arr));
        return arr;
    }

    public static T[] ToArray<T>(this IStorage storage) where T : unmanaged
    {
        if (storage == null)
            return new T[0];

        storage.GetSize(out long storageSize).ThrowIfFailure();

        var arr = new T[storageSize / Unsafe.SizeOf<T>()];
        Span<byte> dest = MemoryMarshal.Cast<T, byte>(arr.AsSpan());

        storage.Read(0, dest);
        return arr;
    }

    public static void WriteAllBytes(this IStorage input, string filename, IProgressReport progress = null)
    {
        input.GetSize(out long inputSize).ThrowIfFailure();

        using (var outFile = new FileStream(filename, FileMode.Create, FileAccess.Write))
        {
            input.CopyToStream(outFile, inputSize, progress);
        }
    }
}

#endif