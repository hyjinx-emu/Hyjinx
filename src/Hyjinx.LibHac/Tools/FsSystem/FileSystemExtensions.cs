using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
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
    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this IFileSystem fileSystem)
    {
        return fileSystem.EnumerateEntries("/", "*");
    }

    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this IFileSystem fileSystem, string path, string searchPattern)
    {
        return fileSystem.EnumerateEntries(path, searchPattern, SearchOptions.RecurseSubdirectories);
    }

    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this IFileSystem fileSystem, string searchPattern, SearchOptions searchOptions)
    {
        return fileSystem.EnumerateEntries("/", searchPattern, searchOptions);
    }

    public static IEnumerable<DirectoryEntryEx> EnumerateEntries(this IFileSystem fileSystem, string path, string searchPattern, SearchOptions searchOptions)
    {
        bool ignoreCase = searchOptions.HasFlag(SearchOptions.CaseInsensitive);
        bool recurse = searchOptions.HasFlag(SearchOptions.RecurseSubdirectories);

        using var directory = new UniqueRef<IDirectory>();

        using (var pathNormalized = new Path())
        {
            InitializeFromString(ref pathNormalized.Ref(), path).ThrowIfFailure();

            fileSystem.OpenDirectory(ref directory.Ref, in pathNormalized, OpenDirectoryMode.All)
                .ThrowIfFailure();
        }

        while (true)
        {
            Unsafe.SkipInit(out DirectoryEntry dirEntry);

            directory.Get.Read(out long entriesRead, SpanHelpers.AsSpan(ref dirEntry)).ThrowIfFailure();
            if (entriesRead == 0)
                break;

            DirectoryEntryEx entry = GetDirectoryEntryEx(ref dirEntry, path);

            if (PathTools.MatchesPattern(searchPattern, entry.Name, ignoreCase))
            {
                yield return entry;
            }

            if (entry.Type != DirectoryEntryType.Directory || !recurse)
                continue;

            IEnumerable<DirectoryEntryEx> subEntries =
                fileSystem.EnumerateEntries(PathTools.Combine(path, entry.Name), searchPattern,
                    searchOptions);

            foreach (DirectoryEntryEx subEntry in subEntries)
            {
                yield return subEntry;
            }
        }
    }

    internal static DirectoryEntryEx GetDirectoryEntryEx(ref DirectoryEntry entry, string parentPath)
    {
        string name = StringUtils.Utf8ZToString(entry.Name);
        string path = PathTools.Combine(parentPath, name);

        var entryEx = new DirectoryEntryEx(name, path, entry.Type, entry.Size)
        {
            Attributes = entry.Attributes
        };

        return entryEx;
    }

    public static IStorage AsStorage(this IFile file) => new FileStorage(file);
    public static Stream AsStream(this IFile file) => new NxFileStream(file, true);

    public static NxFileAttributes ToNxAttributes(this FileAttributes attributes)
    {
        return (NxFileAttributes)(((int)attributes >> 4) & 3);
    }

    public static FileAttributes ToFatAttributes(this NxFileAttributes attributes)
    {
        return (FileAttributes)(((int)attributes & 3) << 4);
    }

    public static FileAttributes ApplyNxAttributes(this FileAttributes attributes, NxFileAttributes nxAttributes)
    {
        // The only 2 bits from FileAttributes that are used in NxFileAttributes
        const int mask = 3 << 4;

        FileAttributes oldAttributes = attributes & (FileAttributes)mask;
        return oldAttributes | nxAttributes.ToFatAttributes();
    }

    public static void CleanDirectoryRecursivelyGeneric(IFileSystem fileSystem, string path)
    {
        IFileSystem fs = fileSystem;

        foreach (DirectoryEntryEx entry in fileSystem.EnumerateEntries(path, "*", SearchOptions.Default))
        {
            string subPath = PathTools.Combine(path, entry.Name);

            using var subPathNormalized = new Path();
            InitializeFromString(ref subPathNormalized.Ref(), subPath).ThrowIfFailure();

            if (entry.Type == DirectoryEntryType.Directory)
            {
                CleanDirectoryRecursivelyGeneric(fileSystem, subPath);
                fs.DeleteDirectory(in subPathNormalized);
            }
            else if (entry.Type == DirectoryEntryType.File)
            {
                fs.DeleteFile(in subPathNormalized);
            }
        }
    }

    public static Result Write(this IFile file, long offset, ReadOnlySpan<byte> source)
    {
        return file.Write(offset, source, WriteOption.None);
    }

    public static bool FileExists(this IFileSystem fs, string path)
    {
        Result res = fs.GetEntryType(out DirectoryEntryType type, path.ToU8Span());

        return (res.IsSuccess() && type == DirectoryEntryType.File);
    }

    private static Result InitializeFromString(ref Path outPath, string path)
    {
        ReadOnlySpan<byte> utf8Path = StringUtils.StringToUtf8(path);

        Result res = outPath.Initialize(utf8Path);
        if (res.IsFailure())
            return res.Miss();

        var pathFlags = new PathFlags();
        pathFlags.AllowEmptyPath();
        outPath.Normalize(pathFlags);
        if (res.IsFailure())
            return res.Miss();

        return Result.Success;
    }
}

[Flags]
public enum SearchOptions
{
    Default = 0,
    RecurseSubdirectories = 1 << 0,
    CaseInsensitive = 1 << 1
}