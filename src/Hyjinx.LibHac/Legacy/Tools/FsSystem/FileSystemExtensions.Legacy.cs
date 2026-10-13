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
    public static Result CopyDirectoryRecursively(IFileSystem destinationFileSystem, IFileSystem sourceFileSystem,
        in Path destinationPath, in Path sourcePath, ref DirectoryEntry dirEntry, Span<byte> workBuffer,
        IProgressReport logger = null, CreateFileOptions option = CreateFileOptions.None)
    {
        static Result OnEnterDir(in Path path, in DirectoryEntry entry,
            ref Utility.FsIterationTaskClosure closure)
        {
            Result res = closure.DestinationPathBuffer.AppendChild(entry.Name);
            if (res.IsFailure())
                return res.Miss();

            res = closure.DestFileSystem.CreateDirectory(in closure.DestinationPathBuffer);
            if (res.IsFailure() && !ResultFs.PathAlreadyExists.Includes(res))
                return res.Miss();

            return Result.Success;
        }

        static Result OnExitDir(in Path path, in DirectoryEntry entry, ref Utility.FsIterationTaskClosure closure)
        {
            return closure.DestinationPathBuffer.RemoveChild();
        }

        Result OnFile(in Path path, in DirectoryEntry entry, ref Utility.FsIterationTaskClosure closure)
        {
            logger?.LogMessage(path.ToString());

            Result result = closure.DestinationPathBuffer.AppendChild(entry.Name);
            if (result.IsFailure())
                return result;

            result = CopyFile(closure.DestFileSystem, closure.SourceFileSystem, in closure.DestinationPathBuffer,
                in path, closure.Buffer, logger, option);
            if (result.IsFailure())
                return result;

            return closure.DestinationPathBuffer.RemoveChild();
        }

        var taskClosure = new Utility.FsIterationTaskClosure();
        taskClosure.Buffer = workBuffer;
        taskClosure.SourceFileSystem = sourceFileSystem;
        taskClosure.DestFileSystem = destinationFileSystem;

        Result res = taskClosure.DestinationPathBuffer.Initialize(destinationPath);
        if (res.IsFailure())
            return res.Miss();

        res = Utility.IterateDirectoryRecursively(sourceFileSystem, in sourcePath, ref dirEntry, OnEnterDir,
            OnExitDir, OnFile, ref taskClosure);

        taskClosure.DestinationPathBuffer.Dispose();
        return res;
    }

    public static Result CopyFile(IFileSystem destFileSystem, IFileSystem sourceFileSystem, in Path destPath,
        in Path sourcePath, Span<byte> workBuffer, IProgressReport logger = null,
        CreateFileOptions option = CreateFileOptions.None)
    {
        // Open source file.
        using var sourceFile = new UniqueRef<IFile>();
        Result res = sourceFileSystem.OpenFile(ref sourceFile.Ref, sourcePath, OpenMode.Read);
        if (res.IsFailure())
            return res.Miss();

        res = sourceFile.Get.GetSize(out long fileSize);
        if (res.IsFailure())
            return res.Miss();

        res = CreateOrOverwriteFile(destFileSystem, in destPath, fileSize, option);
        if (res.IsFailure())
            return res.Miss();

        using var destFile = new UniqueRef<IFile>();
        res = destFileSystem.OpenFile(ref destFile.Ref, in destPath, OpenMode.Write);
        if (res.IsFailure())
            return res.Miss();

        // Read/Write file in work buffer sized chunks.
        long remaining = fileSize;
        long offset = 0;

        logger?.SetTotal(fileSize);

        while (remaining > 0)
        {
            res = sourceFile.Get.Read(out long bytesRead, offset, workBuffer, ReadOption.None);
            if (res.IsFailure())
                return res.Miss();

            res = destFile.Get.Write(offset, workBuffer.Slice(0, (int)bytesRead), WriteOption.None);
            if (res.IsFailure())
                return res.Miss();

            remaining -= bytesRead;
            offset += bytesRead;

            logger?.ReportAdd(bytesRead);
        }

        return Result.Success;
    }

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

    public static int GetEntryCountRecursive(this IFileSystem fs, string path, OpenDirectoryMode mode)
    {
        int count = 0;

        foreach (DirectoryEntryEx entry in fs.EnumerateEntries(path, "*"))
        {
            if (entry.Type == DirectoryEntryType.Directory && (mode & OpenDirectoryMode.Directory) != 0 ||
                entry.Type == DirectoryEntryType.File && (mode & OpenDirectoryMode.File) != 0)
            {
                count++;
            }
        }

        return count;
    }

    public static Result CreateOrOverwriteFile(IFileSystem fileSystem, in Path path, long size,
        CreateFileOptions option = CreateFileOptions.None)
    {
        Result res = fileSystem.CreateFile(in path, size, option);

        if (res.IsFailure())
        {
            if (!ResultFs.PathAlreadyExists.Includes(res))
                return res;

            res = fileSystem.DeleteFile(in path);
            if (res.IsFailure())
                return res.Miss();

            res = fileSystem.CreateFile(in path, size, option);
            if (res.IsFailure())
                return res.Miss();
        }

        return Result.Success;
    }
}

#endif