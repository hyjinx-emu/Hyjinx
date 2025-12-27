#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Util;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Path = LibHac.Fs.Path;
using Utility = LibHac.FsSystem.Utility;

namespace LibHac.Tools.FsSystem;

public static partial class FileSystemExtensions
{
    public static Result CopyDirectory(this IFileSystem sourceFs, IFileSystem destFs, string sourcePath, string destPath,
        IProgressReport logger = null, CreateFileOptions options = CreateFileOptions.None)
    {
        const int bufferSize = 0x100000;

        var directoryEntryBuffer = new DirectoryEntry();

        using var sourcePathNormalized = new Path();
        Result res = InitializeFromString(ref sourcePathNormalized.Ref(), sourcePath);
        if (res.IsFailure())
            return res.Miss();

        using var destPathNormalized = new Path();
        res = InitializeFromString(ref destPathNormalized.Ref(), destPath);
        if (res.IsFailure())
            return res.Miss();

        byte[] workBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            return CopyDirectoryRecursively(destFs, sourceFs, in destPathNormalized, in sourcePathNormalized,
                ref directoryEntryBuffer, workBuffer, logger, options);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(workBuffer);
            logger?.SetTotal(0);
        }
    }

    public static void Extract(this IFileSystem source, string destinationPath, IProgressReport logger = null)
    {
        var destFs = new LocalFileSystem(destinationPath);

        source.CopyDirectory(destFs, "/", "/", logger).ThrowIfFailure();
    }
    
    public static void CopyTo(this IFile file, IFile dest, IProgressReport logger = null)
    {
        const int bufferSize = 0x8000;

        file.GetSize(out long fileSize).ThrowIfFailure();

        logger?.SetTotal(fileSize);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            long inOffset = 0;

            // todo: use result for loop condition
            while (true)
            {
                file.Read(out long bytesRead, inOffset, buffer).ThrowIfFailure();
                if (bytesRead == 0)
                    break;

                dest.Write(inOffset, buffer.AsSpan(0, (int)bytesRead)).ThrowIfFailure();
                inOffset += bytesRead;
                logger?.ReportAdd(bytesRead);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            logger?.SetTotal(0);
        }
    }
    
    public static Stream AsStream(this IFile file, OpenMode mode, bool keepOpen) => new NxFileStream(file, mode, keepOpen);

    public static IFile AsIFile(this Stream stream, OpenMode mode) => new StreamFile(stream, mode);

    public static int GetEntryCount(this IFileSystem fs, OpenDirectoryMode mode)
    {
        return GetEntryCountRecursive(fs, "/", mode);
    }
    
    public static void SetConcatenationFileAttribute(this IFileSystem fs, string path)
    {
        using var pathNormalized = new Path();
        InitializeFromString(ref pathNormalized.Ref(), path).ThrowIfFailure();

        fs.QueryEntry(Span<byte>.Empty, Span<byte>.Empty, QueryId.SetConcatenationFileAttribute, in pathNormalized);
    }
    
    public static Result Read(this IFile file, out long bytesRead, long offset, Span<byte> destination)
    {
        return file.Read(out bytesRead, offset, destination, ReadOption.None);
    }
    
    public static bool DirectoryExists(this IFileSystem fs, string path)
    {
        Result res = fs.GetEntryType(out DirectoryEntryType type, path.ToU8Span());

        return (res.IsSuccess() && type == DirectoryEntryType.Directory);
    }
    
    public static Result EnsureDirectoryExists(this IFileSystem fs, string path)
    {
        using var pathNormalized = new Path();
        Result res = InitializeFromString(ref pathNormalized.Ref(), path);
        if (res.IsFailure())
            return res.Miss();

        return Utility.EnsureDirectory(fs, in pathNormalized);
    }
}

#endif