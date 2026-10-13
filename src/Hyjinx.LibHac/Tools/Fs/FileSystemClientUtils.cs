using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using System.Collections.Generic;

namespace LibHac.Tools.Fs;

public static partial class FileSystemClientUtils
{
    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this FileSystemClient fs, string path, string searchPattern, SearchOptions searchOptions)
    {
        bool ignoreCase = searchOptions.HasFlag(SearchOptions.CaseInsensitive);
        bool recurse = searchOptions.HasFlag(SearchOptions.RecurseSubdirectories);

        fs.OpenDirectory(out DirectoryHandle sourceHandle, path.ToU8Span(), OpenDirectoryMode.All).ThrowIfFailure();

        try
        {
            while (true)
            {
                DirectoryEntry dirEntry = default;

                fs.ReadDirectory(out long entriesRead, SpanHelpers.AsSpan(ref dirEntry), sourceHandle);
                if (entriesRead == 0)
                    break;

                DirectoryEntryEx entry = FileSystemExtensions.GetDirectoryEntryEx(ref dirEntry, path);

                if (PathTools.MatchesPattern(searchPattern, entry.Name, ignoreCase))
                {
                    yield return entry;
                }

                if (entry.Type != DirectoryEntryType.Directory || !recurse)
                    continue;

                IEnumerable<DirectoryEntryEx> subEntries =
                    fs.EnumerateEntries(PathTools.Combine(path, entry.Name), searchPattern, searchOptions);

                foreach (DirectoryEntryEx subEntry in subEntries)
                {
                    yield return subEntry;
                }
            }
        }
        finally
        {
            if (sourceHandle.IsValid)
                fs.CloseDirectory(sourceHandle);
        }
    }

    public static void EnsureDirectoryExists(this FileSystemClient fs, string path)
    {
        path = PathTools.Normalize(path);
        if (fs.DirectoryExists(path))
            return;

        PathTools.GetMountNameLength(path, out int mountNameLength).ThrowIfFailure();

        // Find the first subdirectory in the path that doesn't exist
        int i;
        for (i = path.Length - 1; i > mountNameLength + 2; i--)
        {
            if (path[i] == '/')
            {
                string subPath = path.Substring(0, i);

                if (fs.DirectoryExists(subPath))
                {
                    break;
                }
            }
        }

        // path[i] will be a '/', so skip that character
        i++;

        // loop until `path.Length - 1` so CreateDirectory won't be called multiple
        // times on path if the last character in the path is a '/'
        for (; i < path.Length - 1; i++)
        {
            if (path[i] == '/')
            {
                string subPath = path.Substring(0, i);

                fs.CreateDirectory(subPath.ToU8Span());
            }
        }

        fs.CreateDirectory(path.ToU8Span());
    }

    private static bool DirectoryExists(this FileSystemClient fs, string path)
    {
        Result res = fs.GetEntryType(out DirectoryEntryType type, path.ToU8Span());

        return (res.IsSuccess() && type == DirectoryEntryType.Directory);
    }

    public static Result CreateOrOverwriteFile(this FileSystemClient fs, string path, long size)
    {
        return fs.CreateOrOverwriteFile(path, size, CreateFileOptions.None);
    }

    public static Result CreateOrOverwriteFile(this FileSystemClient fs, string path, long size, CreateFileOptions options)
    {
        path = PathTools.Normalize(path);
        var u8Path = path.ToU8Span();

        if (fs.FileExists(path))
        {
            Result res = fs.DeleteFile(u8Path);
            if (res.IsFailure())
                return res.Miss();
        }

        return fs.CreateFile(u8Path, size, CreateFileOptions.None);
    }

    private static bool FileExists(this FileSystemClient fs, string path)
    {
        Result res = fs.GetEntryType(out DirectoryEntryType type, path.ToU8Span());

        return (res.IsSuccess() && type == DirectoryEntryType.File);
    }
}