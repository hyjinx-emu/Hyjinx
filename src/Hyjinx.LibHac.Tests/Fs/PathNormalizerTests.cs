// ReSharper disable InconsistentNaming

using LibHac.Common;
using LibHac.Fs;
using LibHac.Util;
using Xunit;

namespace LibHac.Tests.Fs;

public class PathNormalizerTests
{
    public static TheoryData<string, bool, bool, bool, string, long, Result> TestData_Normalize => new()
    {
        { @"/aa/bb/c/", false, true, false, @"/aa/bb/c", 8, Result.Success },
        { @"aa/bb/c/", false, false, false, @"", 0, ResultFs.InvalidPathFormat.Value },
        { @"aa/bb/c/", false, true, false, @"/aa/bb/c", 8, Result.Success },
        { @"mount:a/b", false, true, false, @"/", 0, ResultFs.InvalidCharacter.Value },
        { @"mo|unt:a/b", false, true, true, @"/mo|unt:a/b", 11, Result.Success },
        { @"/aa/bb/../..", true, false, false, @"/", 1, Result.Success },
        { @"/aa/bb/../../..", true, false, false, @"/", 1, Result.Success },
        { @"/aa/bb/../../..", false, false, false, @"/aa/bb/", 0, ResultFs.DirectoryUnobtainable.Value },
        { @"aa/bb/../../..", true, true, false, @"/", 1, Result.Success },
        { @"aa/bb/../../..", false, true, false, @"/aa/bb/", 0, ResultFs.DirectoryUnobtainable.Value },
        { @"mount:a/b", false, true, true, @"/mount:a/b", 10, Result.Success },
        { @"/a|/bb/cc", false, false, true, @"/a|/bb/cc", 9, Result.Success },
        { @"/>a/bb/cc", false, false, true, @"/>a/bb/cc", 9, Result.Success },
        { @"/aa/.</cc", false, false, true, @"/aa/.</cc", 9, Result.Success },
        { @"/aa/..</cc", false, false, true, @"/aa/..</cc", 10, Result.Success },
        { @"", false, false, false, @"", 0, ResultFs.InvalidPathFormat.Value },
        { @"/", false, false, false, @"/", 1, Result.Success },
        { @"/.", false, false, false, @"/", 1, Result.Success },
        { @"/./", false, false, false, @"/", 1, Result.Success },
        { @"/..", false, false, false, @"/", 0, ResultFs.DirectoryUnobtainable.Value },
        { @"//.", false, false, false, @"/", 1, Result.Success },
        { @"/ ..", false, false, false, @"/ ..", 4, Result.Success },
        { @"/.. /", false, false, false, @"/.. ", 4, Result.Success },
        { @"/. /.", false, false, false, @"/. ", 3, Result.Success },
        { @"/aa/bb/cc/dd/./.././../..", false, false, false, @"/aa", 3, Result.Success },
        { @"/aa/bb/cc/dd/./.././../../..", false, false, false, @"/", 1, Result.Success },
        { @"/./aa/./bb/./cc/./dd/.", false, false, false, @"/aa/bb/cc/dd", 12, Result.Success },
        { @"/aa\bb/cc", false, false, false, @"/aa\bb/cc", 9, Result.Success },
        { @"/aa\bb/cc", false, false, false, @"/aa\bb/cc", 9, Result.Success },
        { @"/a|/bb/cc", false, false, false, @"/", 0, ResultFs.InvalidCharacter.Value },
        { @"/>a/bb/cc", false, false, false, @"/", 0, ResultFs.InvalidCharacter.Value },
        { @"/aa/.</cc", false, false, false, @"/aa/", 0, ResultFs.InvalidCharacter.Value },
        { @"/aa/..</cc", false, false, false, @"/aa/", 0, ResultFs.InvalidCharacter.Value },
        { @"\\aa/bb/cc", false, false, false, @"", 0, ResultFs.InvalidPathFormat.Value },
        { @"\\aa\bb\cc", false, false, false, @"", 0, ResultFs.InvalidPathFormat.Value },
        { @"/aa/bb/..\cc", false, false, false, @"/aa/cc", 6, Result.Success },
        { @"/aa/bb\..\cc", false, false, false, @"/aa/cc", 6, Result.Success },
        { @"/aa/bb\..", false, false, false, @"/aa", 3, Result.Success },
        { @"/aa\bb/../cc", false, false, false, @"/cc", 3, Result.Success }
    };

    [Theory, MemberData(nameof(TestData_Normalize))]
    public static void Normalize(string path, bool isWindowsPath, bool isDriveRelativePath, bool allowAllCharacters,
        string expectedNormalized, long expectedLength, Result expectedResult)
    {
        byte[] buffer = new byte[0x301];

        Result result = PathNormalizer.Normalize(buffer, out int normalizedLength, path.ToU8Span(), isWindowsPath,
            isDriveRelativePath, allowAllCharacters);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedNormalized, StringUtils.Utf8ZToString(buffer));
        Assert.Equal(expectedLength, normalizedLength);
    }

    public static TheoryData<string, int, string, long, Result> TestData_Normalize_SmallBuffer => new()
    {
        { @"/aa/bb/cc/", 7, @"/aa/bb", 6, ResultFs.TooLongPath.Value },
        { @"/aa/bb/cc/", 8, @"/aa/bb/", 7, ResultFs.TooLongPath.Value },
        { @"/aa/bb/cc/", 9, @"/aa/bb/c", 8, ResultFs.TooLongPath.Value },
        { @"/aa/bb/cc/", 10, @"/aa/bb/cc", 9, Result.Success },
        { @"/aa/bb/cc", 9, @"/aa/bb/c", 8, ResultFs.TooLongPath.Value },
        { @"/aa/bb/cc", 10, @"/aa/bb/cc", 9, Result.Success },
        { @"/./aa/./bb/./cc", 9, @"/aa/bb/c", 8, ResultFs.TooLongPath.Value },
        { @"/./aa/./bb/./cc", 10, @"/aa/bb/cc", 9, Result.Success },
        { @"/aa/bb/cc/../../..", 9, @"/aa/bb/c", 8, ResultFs.TooLongPath.Value },
        { @"/aa/bb/cc/../../..", 10, @"/aa/bb/cc", 9, ResultFs.TooLongPath.Value },
        { @"/aa/bb/.", 7, @"/aa/bb", 6, ResultFs.TooLongPath.Value },
        { @"/aa/bb/./", 7, @"/aa/bb", 6, ResultFs.TooLongPath.Value },
        { @"/aa/bb/..", 8, @"/aa", 3, Result.Success },
        { @"/aa/bb", 1, @"", 0, ResultFs.TooLongPath.Value },
        { @"/aa/bb", 2, @"/", 1, ResultFs.TooLongPath.Value },
        { @"/aa/bb", 3, @"/a", 2, ResultFs.TooLongPath.Value },
        { @"aa/bb", 1, @"", 0, ResultFs.InvalidPathFormat.Value }
    };

    [Theory, MemberData(nameof(TestData_Normalize_SmallBuffer))]
    public static void Normalize_SmallBuffer(string path, int bufferLength, string expectedNormalized, long expectedLength, Result expectedResult)
    {
        byte[] buffer = new byte[bufferLength];

        Result result = PathNormalizer.Normalize(buffer, out int normalizedLength, path.ToU8Span(), false, false);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedNormalized, StringUtils.Utf8ZToString(buffer));
        Assert.Equal(expectedLength, normalizedLength);
    }

    public static TheoryData<string, bool, bool, long, Result> TestData_IsNormalized => new()
    {
        { @"/aa/bb/c/", false, false, 9, Result.Success },
        { @"aa/bb/c/", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"aa/bb/c/", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"mount:a/b", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"mo|unt:a/b", true, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"/aa/bb/../..", false, false, 0, Result.Success },
        { @"/aa/bb/../../..", false, false, 0, Result.Success },
        { @"/aa/bb/../../..", false, false, 0, Result.Success },
        { @"aa/bb/../../..", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"aa/bb/../../..", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"mount:a/b", true, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"/a|/bb/cc", true, true, 9, Result.Success },
        { @"/>a/bb/cc", true, true, 9, Result.Success },
        { @"/aa/.</cc", true, true, 9, Result.Success },
        { @"/aa/..</cc", true, true, 10, Result.Success },
        { @"", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"/", false, true, 1, Result.Success },
        { @"/.", false, false, 2, Result.Success },
        { @"/./", false, false, 0, Result.Success },
        { @"/..", false, false, 3, Result.Success },
        { @"//.", false, false, 0, Result.Success },
        { @"/ ..", false, true, 4, Result.Success },
        { @"/.. /", false, false, 5, Result.Success },
        { @"/. /.", false, false, 5, Result.Success },
        { @"/aa/bb/cc/dd/./.././../..", false, false, 0, Result.Success },
        { @"/aa/bb/cc/dd/./.././../../..", false, false, 0, Result.Success },
        { @"/./aa/./bb/./cc/./dd/.", false, false, 0, Result.Success },
        { @"/aa\bb/cc", false, true, 9, Result.Success },
        { @"/aa\bb/cc", false, true, 9, Result.Success },
        { @"/a|/bb/cc", false, false, 0, ResultFs.InvalidCharacter.Value },
        { @"/>a/bb/cc", false, false, 0, ResultFs.InvalidCharacter.Value },
        { @"/aa/.</cc", false, false, 0, ResultFs.InvalidCharacter.Value },
        { @"/aa/..</cc", false, false, 0, ResultFs.InvalidCharacter.Value },
        { @"\\aa/bb/cc", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"\\aa\bb\cc", false, false, 0, ResultFs.InvalidPathFormat.Value },
        { @"/aa/bb/..\cc", false, true, 12, Result.Success },
        { @"/aa/bb\..\cc", false, true, 12, Result.Success },
        { @"/aa/bb\..", false, true, 9, Result.Success },
        { @"/aa\bb/../cc", false, false, 0, Result.Success }
    };

    [Theory, MemberData(nameof(TestData_IsNormalized))]
    public static void IsNormalized(string path, bool allowAllCharacters, bool expectedIsNormalized, long expectedLength, Result expectedResult)
    {
        Result result = PathNormalizer.IsNormalized(out bool isNormalized, out int length, path.ToU8Span(), allowAllCharacters);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedLength, length);

        if (result.IsSuccess())
        {
            Assert.Equal(expectedIsNormalized, isNormalized);
        }
    }
}