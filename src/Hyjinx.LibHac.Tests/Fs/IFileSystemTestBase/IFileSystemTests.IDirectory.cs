using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Util;
using System;
using NUnit.Framework;

namespace LibHac.Tests.Fs.IFileSystemTestBase;

public abstract partial class IFileSystemTests
{
    [Test]
    public void IDirectoryRead_EmptyFs_NoEntriesAreRead()
    {
        IFileSystem fs = CreateFileSystem();
        Span<DirectoryEntry> entries = stackalloc DirectoryEntry[1];

        using var directory = new UniqueRef<IDirectory>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenDirectory(ref directory.Ref, "/", OpenDirectoryMode.All));

        LibHac.Tests.Fs.FsTestAssert.Success(directory.Get.Read(out long entriesRead, entries));
        Assert.That(entriesRead, Is.EqualTo(0));
    }

    [Test]
    public void IDirectoryGetEntryCount_EmptyFs_EntryCountIsZero()
    {
        IFileSystem fs = CreateFileSystem();

        using var directory = new UniqueRef<IDirectory>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenDirectory(ref directory.Ref, "/", OpenDirectoryMode.All));

        LibHac.Tests.Fs.FsTestAssert.Success(directory.Get.GetEntryCount(out long entryCount));
        Assert.That(entryCount, Is.EqualTo(0));
    }

    [Test]
    public void IDirectoryRead_AllEntriesAreReturned()
    {
        IFileSystem fs = CreateFileSystem();
        fs.CreateDirectory("/dir");
        fs.CreateDirectory("/dir/dir1");
        fs.CreateFile("/dir/dir1/file1", 0, CreateFileOptions.None);
        fs.CreateFile("/dir/file1", 0, CreateFileOptions.None);
        fs.CreateFile("/dir/file2", 0, CreateFileOptions.None);

        using var dir = new UniqueRef<IDirectory>();
        LibHac.Tests.Fs.FsTestAssert.Success(fs.OpenDirectory(ref dir.Ref, "/dir", OpenDirectoryMode.All));

        var entry1 = new DirectoryEntry();
        var entry2 = new DirectoryEntry();
        var entry3 = new DirectoryEntry();
        var entry4 = new DirectoryEntry();

        LibHac.Tests.Fs.FsTestAssert.Success(dir.Get.Read(out long entriesRead1, SpanHelpers.AsSpan(ref entry1)));
        LibHac.Tests.Fs.FsTestAssert.Success(dir.Get.Read(out long entriesRead2, SpanHelpers.AsSpan(ref entry2)));
        LibHac.Tests.Fs.FsTestAssert.Success(dir.Get.Read(out long entriesRead3, SpanHelpers.AsSpan(ref entry3)));
        LibHac.Tests.Fs.FsTestAssert.Success(dir.Get.Read(out long entriesRead4, SpanHelpers.AsSpan(ref entry4)));

        Assert.That(entriesRead1, Is.EqualTo(1));
        Assert.That(entriesRead2, Is.EqualTo(1));
        Assert.That(entriesRead3, Is.EqualTo(1));
        Assert.That(entriesRead4, Is.EqualTo(0));

        bool dir1Read = false;
        bool file1Read = false;
        bool file2Read = false;

        // Entries are not guaranteed to be in any particular order
        CheckEntry(ref entry1);
        CheckEntry(ref entry2);
        CheckEntry(ref entry3);

        Assert.That(dir1Read, Is.True);
        Assert.That(file1Read, Is.True);
        Assert.That(file2Read, Is.True);

        void CheckEntry(ref DirectoryEntry entry)
        {
            switch (StringUtils.Utf8ZToString(entry.Name))
            {
                case "dir1":
                    Assert.That(dir1Read, Is.False);
                    Assert.That(entry.Type, Is.EqualTo(DirectoryEntryType.Directory));

                    dir1Read = true;
                    break;

                case "file1":
                    Assert.That(file1Read, Is.False);
                    Assert.That(entry.Type, Is.EqualTo(DirectoryEntryType.File));

                    file1Read = true;
                    break;

                case "file2":
                    Assert.That(file2Read, Is.False);
                    Assert.That(entry.Type, Is.EqualTo(DirectoryEntryType.File));

                    file2Read = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}