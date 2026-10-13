#if IS_LEGACY_ENABLED

using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace LibHac.Tools.Fs;

partial class FileSystemClientUtils
{
    public static Result CopyDirectory(this FileSystemClient fs, string sourcePath, string destPath,
        CreateFileOptions options = CreateFileOptions.None, IProgressReport logger = null)
    {
        Result res = fs.OpenDirectory(out DirectoryHandle sourceHandle, sourcePath.ToU8Span(), OpenDirectoryMode.All);
        if (res.IsFailure())
            return res.Miss();

        try
        {
            foreach (DirectoryEntryEx entry in fs.EnumerateEntries(sourcePath, "*", SearchOptions.Default))
            {
                string subSrcPath = PathTools.Normalize(PathTools.Combine(sourcePath, entry.Name));
                string subDstPath = PathTools.Normalize(PathTools.Combine(destPath, entry.Name));

                if (entry.Type == DirectoryEntryType.Directory)
                {
                    fs.EnsureDirectoryExists(subDstPath);

                    res = fs.CopyDirectory(subSrcPath, subDstPath, options, logger);
                    if (res.IsFailure())
                        return res.Miss();
                }

                if (entry.Type == DirectoryEntryType.File)
                {
                    logger?.LogMessage(subSrcPath);
                    fs.CreateOrOverwriteFile(subDstPath, entry.Size, options);

                    res = fs.CopyFile(subSrcPath, subDstPath, logger);
                    if (res.IsFailure())
                        return res.Miss();
                }
            }
        }
        finally
        {
            if (sourceHandle.IsValid)
                fs.CloseDirectory(sourceHandle);
        }

        return Result.Success;
    }

    public static Result CopyFile(this FileSystemClient fs, string sourcePath, string destPath, IProgressReport logger = null)
    {
        Result res = fs.OpenFile(out FileHandle sourceHandle, sourcePath.ToU8Span(), OpenMode.Read);
        if (res.IsFailure())
            return res.Miss();

        try
        {
            res = fs.OpenFile(out FileHandle destHandle, destPath.ToU8Span(),
                OpenMode.Write | OpenMode.AllowAppend);
            if (res.IsFailure())
                return res.Miss();

            try
            {
                const int maxBufferSize = 0x10000;

                res = fs.GetFileSize(out long fileSize, sourceHandle);
                if (res.IsFailure())
                    return res.Miss();

                int bufferSize = (int)Math.Min(maxBufferSize, fileSize);

                logger?.SetTotal(fileSize);

                byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    for (long offset = 0; offset < fileSize; offset += bufferSize)
                    {
                        int toRead = (int)Math.Min(fileSize - offset, bufferSize);
                        Span<byte> buf = buffer.AsSpan(0, toRead);

                        res = fs.ReadFile(out long _, sourceHandle, offset, buf);
                        if (res.IsFailure())
                            return res.Miss();

                        res = fs.WriteFile(destHandle, offset, buf, WriteOption.None);
                        if (res.IsFailure())
                            return res.Miss();

                        logger?.ReportAdd(toRead);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    logger?.SetTotal(0);
                }

                res = fs.FlushFile(destHandle);
                if (res.IsFailure())
                    return res.Miss();
            }
            finally
            {
                if (destHandle.IsValid)
                    fs.CloseFile(destHandle);
            }
        }
        finally
        {
            if (sourceHandle.IsValid)
                fs.CloseFile(sourceHandle);
        }

        return Result.Success;
    }

    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this FileSystemClient fs, string path)
    {
        return fs.EnumerateEntries(path, "*");
    }

    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this FileSystemClient fs, string path, string searchPattern)
    {
        return fs.EnumerateEntries(path, searchPattern, SearchOptions.RecurseSubdirectories);
    }
}

#endif