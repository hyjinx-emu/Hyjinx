using LibHac;
using NUnit.Framework;

namespace LibHac.Tests.Fs;

internal static class FsTestAssert
{
    public static void Success(Result result)
    {
        Assert.That(result.IsSuccess(), Is.True, result.ToString());
    }

    public static void Failure(Result result)
    {
        Assert.That(result.IsSuccess(), Is.False, result.ToString());
    }

    public static void Result(Result.Base expected, Result actual)
    {
        Assert.That(expected.Includes(actual), Is.True, actual.ToString());
    }
}