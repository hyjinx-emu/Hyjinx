using Hyjinx.Common.Configuration;
using LibHac.Fs;
using LibHac.Ncm;
using System;
using static Hyjinx.HLE.FileSystem.VirtualFileSystem;
using Path = System.IO.Path;

namespace Hyjinx.HLE.FileSystem;

internal static class ContentPath
{
    public const string SystemContent = "@SystemContent";
    public const string UserContent = "@UserContent";
    public const string SdCardContent = "@SdCardContent";
    public const string SdCard = "@Sdcard";
    public const string CalibFile = "@CalibFile";
    public const string Safe = "@Safe";
    public const string User = "@User";
    public const string System = "@System";
    public const string Host = "@Host";
    public const string GamecardApp = "@GcApp";
    public const string GamecardContents = "@GcS00000001";
    public const string GamecardUpdate = "@upp";
    public const string RegisteredUpdate = "@RegUpdate";

    public const string Nintendo = "Nintendo";
    public const string Contents = "Contents";

    public static bool TryGetRealPath(string switchContentPath, out string realPath)
    {
        realPath = switchContentPath switch
        {
            SystemContent => Path.Combine(AppDataManager.BaseDirPath, SystemNandPath, Contents),
            UserContent => Path.Combine(AppDataManager.BaseDirPath, UserNandPath, Contents),
            SdCardContent => Path.Combine(GetSdCardPath(), Nintendo, Contents),
            System => Path.Combine(AppDataManager.BaseDirPath, SystemNandPath),
            User => Path.Combine(AppDataManager.BaseDirPath, UserNandPath),
            _ => null,
        };

        return realPath != null;
    }

    public static string GetContentPath(ContentStorageId contentStorageId)
    {
        return contentStorageId switch
        {
            ContentStorageId.System => SystemContent,
            ContentStorageId.User => UserContent,
            ContentStorageId.SdCard => SdCardContent,
            _ => throw new NotSupportedException($"Content Storage Id \"`{contentStorageId}`\" is not supported."),
        };
    }

    public static bool TryGetContentPath(StorageId storageId, out string contentPath)
    {
        contentPath = storageId switch
        {
            StorageId.BuiltInSystem => SystemContent,
            StorageId.BuiltInUser => UserContent,
            StorageId.SdCard => SdCardContent,
            _ => null,
        };

        return contentPath != null;
    }

    public static StorageId GetStorageId(string contentPathString)
    {
        return contentPathString.Split(':')[0] switch
        {
            SystemContent or
            System => StorageId.BuiltInSystem,
            UserContent or
            User => StorageId.BuiltInUser,
            SdCardContent => StorageId.SdCard,
            Host => StorageId.Host,
            GamecardApp or
            GamecardContents or
            GamecardUpdate => StorageId.GameCard,
            _ => StorageId.None,
        };
    }
}