using LibHac.Common;
using LibHac.Fs.Impl;
using LibHac.Os;
using System;
using System.Runtime.CompilerServices;
using static LibHac.Fs.Impl.AccessLogStrings;

namespace LibHac.Fs.Fsa;

/// <summary>
/// Contains functions for interacting with opened directories.
/// </summary>
/// <remarks>Based on nnSdk 13.4.0 (FS 13.1.0)</remarks>
[SkipLocalsInit]
public static class UserDirectory
{
    private static DirectoryAccessor Get(DirectoryHandle handle)
    {
        return handle.Directory;
    }

    public static Result ReadDirectory(this FileSystemClient fs, out long entriesRead,
        Span<DirectoryEntry> entryBuffer, DirectoryHandle handle)
    {
        Result res;

        if (fs.Impl.IsEnabledAccessLog() && fs.Impl.IsEnabledHandleAccessLog(handle))
        {
            Tick start = fs.Hos.Os.GetSystemTick();
            res = Get(handle).Read(out entriesRead, entryBuffer);
            Tick end = fs.Hos.Os.GetSystemTick();

            Span<byte> buffer = stackalloc byte[0x50];
            var sb = new U8StringBuilder(buffer, true);

            sb.Append(LogEntryBufferCount).AppendFormat(entryBuffer.Length)
              .Append(LogEntryCount).AppendFormat(entriesRead);
            fs.Impl.OutputAccessLog(res, start, end, handle, new U8Span(sb.Buffer));
        }
        else
        {
            res = Get(handle).Read(out entriesRead, entryBuffer);
        }

        fs.Impl.AbortIfNeeded(res);
        return res;
    }

    public static Result GetDirectoryEntryCount(this FileSystemClient fs, out long count, DirectoryHandle handle)
    {
        Result res;

        if (fs.Impl.IsEnabledAccessLog() && fs.Impl.IsEnabledHandleAccessLog(handle))
        {
            Tick start = fs.Hos.Os.GetSystemTick();
            res = Get(handle).GetEntryCount(out count);
            Tick end = fs.Hos.Os.GetSystemTick();

            Span<byte> buffer = stackalloc byte[0x50];
            var sb = new U8StringBuilder(buffer, true);

            sb.Append(LogEntryCount).AppendFormat(count);
            fs.Impl.OutputAccessLog(res, start, end, handle, new U8Span(sb.Buffer));
        }
        else
        {
            res = Get(handle).GetEntryCount(out count);
        }

        fs.Impl.AbortIfNeeded(res);
        return res;
    }

    public static void CloseDirectory(this FileSystemClient fs, DirectoryHandle handle)
    {
        if (fs.Impl.IsEnabledAccessLog() && fs.Impl.IsEnabledHandleAccessLog(handle))
        {
            Tick start = fs.Hos.Os.GetSystemTick();
            Get(handle).Dispose();
            Tick end = fs.Hos.Os.GetSystemTick();

            fs.Impl.OutputAccessLog(Result.Success, start, end, handle, U8Span.Empty);
        }
        else
        {
            Get(handle).Dispose();
        }
    }
}