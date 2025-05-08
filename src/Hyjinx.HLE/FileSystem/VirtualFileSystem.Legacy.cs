#if IS_LEGACY_ENABLED

using Hyjinx.Common.Configuration;
using System;
using System.IO;

namespace Hyjinx.HLE.FileSystem;

partial class VirtualFileSystem
{
    public EmulatedGameCard GameCard { get; private set; }

    public static string? SystemPathToSwitchPath(string systemPath)
    {
        string baseSystemPath = AppDataManager.BaseDirPath + Path.DirectorySeparatorChar;

        if (systemPath.StartsWith(baseSystemPath))
        {
            string rawPath = systemPath.Replace(baseSystemPath, "");
            int firstSeparatorOffset = rawPath.IndexOf(Path.DirectorySeparatorChar);

            if (firstSeparatorOffset == -1)
            {
                return $"{rawPath}:/";
            }

            var basePath = rawPath.AsSpan(0, firstSeparatorOffset);
            var fileName = rawPath.AsSpan(firstSeparatorOffset + 1);

            return $"{basePath}:/{fileName}";
        }

        return null;
    }
}

#endif
