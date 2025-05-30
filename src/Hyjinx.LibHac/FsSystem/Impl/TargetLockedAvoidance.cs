#nullable enable
using LibHac.Fs;
using LibHac.Os;
using System;

namespace LibHac.FsSystem.Impl;

internal static class TargetLockedAvoidance
{
    private const int RetryCount = 25;
    private const int SleepTimeMs = 2;

    // Allow usage outside of a Horizon context by using standard .NET APIs
    public static Result RetryToAvoidTargetLocked(Func<Result> func, FileSystemClient? fs = null)
    {
        Result res = func();

        for (int i = 0; i < RetryCount && ResultFs.TargetLocked.Includes(res); i++)
        {
            if (fs is null)
            {
                System.Threading.Thread.Sleep(SleepTimeMs);
            }
            else
            {
                fs.Hos.Os.SleepThread(TimeSpan.FromMilliSeconds(SleepTimeMs));
            }

            res = func();
        }

        return res;
    }
}