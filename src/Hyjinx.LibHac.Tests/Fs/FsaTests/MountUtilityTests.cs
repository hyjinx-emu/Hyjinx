using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;
using LibHac.Tests.Fs.FileSystemClientTests;
using LibHac.Tools.Fs;
using LibHac.Util;
using System;
using NUnit.Framework;

namespace LibHac.Tests.Fs.FsaTests;

public class MountUtilityTests
{
    [TestCase("0123456789ABCDE", "0123456789ABCDE:/")]
    [TestCase("01234", "01234:/")]
    public void GetMountName_ValidName_ReturnsSuccess(string mountName, string path)
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountSdCard(mountName.ToU8Span()));
        LibHac.Tests.Fs.FsTestAssert.Success(fs.GetEntryType(out _, path.ToU8Span()));
    }
    [TestCase("01234", "01234")]
    [TestCase("0123456789ABCDE", "0123456789ABCDE")]
    [TestCase("01234", "0123456789ABCDEF")]
    [TestCase("01234", "0123456789ABCDEF:/")]
    public void GetMountName_InvalidName_ReturnsInvalidMountName(string mountName, string path)
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        LibHac.Tests.Fs.FsTestAssert.Success(fs.MountSdCard(mountName.ToU8Span()));
        LibHac.Tests.Fs.FsTestAssert.Result(ResultFs.InvalidMountName, fs.GetEntryType(out _, path.ToU8Span()));
    }

    private class TestCommonMountNameGenerator : ICommonMountNameGenerator
    {
        public void Dispose() { }

        public Result GenerateCommonMountName(Span<byte> nameBuffer)
        {
            ReadOnlySpan<byte> mountName = "@Test"u8;

            // Add 2 for the mount name separator and null terminator
            int requiredNameBufferSize = StringUtils.GetLength(mountName, PathTool.MountNameLengthMax) + 2;

            Assert.That(nameBuffer.Length >= requiredNameBufferSize, Is.True);

            var sb = new U8StringBuilder(nameBuffer);
            sb.Append(mountName).Append(StringTraits.DriveSeparator);

            Assert.That(requiredNameBufferSize - 1, Is.EqualTo(sb.Length));

            return Result.Success;
        }
    }

    [Test]
    public void ConvertToFsCommonPath_MountedWithCommonPath_ReturnsCommonPath()
    {
        FileSystemClient fs = FileSystemServerFactory.CreateClient(true);

        using var fileSystem = new UniqueRef<IFileSystem>(new InMemoryFileSystem());
        using var mountNameGenerator = new UniqueRef<ICommonMountNameGenerator>(new TestCommonMountNameGenerator());
        LibHac.Tests.Fs.FsTestAssert.Success(fs.Register("mount"u8, ref fileSystem.Ref, ref mountNameGenerator.Ref));

        byte[] outputPath = new byte[100];
        LibHac.Tests.Fs.FsTestAssert.Success(fs.ConvertToFsCommonPath(new U8SpanMutable(outputPath), "mount:/entry1/entry2"u8));

        string expected = "@Test:/entry1/entry2";
        string actual = StringUtils.Utf8ZToString(outputPath);
        Assert.That(actual, Is.EqualTo(expected));
    }
}