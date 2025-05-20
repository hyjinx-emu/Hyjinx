using Hyjinx.UI.Common.AutoConfiguration;

namespace Hyjinx.UI.Common.Helper;

public static class CommandLineState
{
    public static bool? OverrideDockedMode { get; private set; }
    public static bool? OverrideHardwareAcceleration { get; private set; }
    public static string? OverrideGraphicsBackend { get; private set; }
    public static string? OverrideHideCursor { get; private set; }
    public static string? BaseDirPathArg { get; private set; }
    public static string? Profile { get; private set; }
    public static string? LaunchPathArg { get; private set; }
    public static string? LaunchApplicationId { get; private set; }
    public static bool StartFullscreenArg { get; private set; }
    public static string? OverrideConfigFile { get; private set; }

    public static void ParseArguments(LaunchOptions launchOptions)
    {
        OverrideDockedMode = launchOptions.OverrideDockedMode;
        OverrideHardwareAcceleration = launchOptions.OverrideHardwareAcceleration;
        OverrideGraphicsBackend = launchOptions.OverrideGraphicsBackend;
        OverrideHideCursor = launchOptions.OverrideHideCursor;
        BaseDirPathArg = launchOptions.BaseDirPathArg;
        Profile = launchOptions.Profile;
        LaunchPathArg = launchOptions.LaunchPathArg;
        LaunchApplicationId = launchOptions.LaunchApplicationId;
        StartFullscreenArg = launchOptions.StartFullscreenArg;
        OverrideConfigFile = launchOptions.OverrideConfigFile;
    }
}