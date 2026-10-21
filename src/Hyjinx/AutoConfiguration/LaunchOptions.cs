using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.AutoConfiguration;

public record LaunchOptions
{
    [ConfigurationKeyName("-docked-mode")]
    public bool? OverrideDockedMode { get; init; }

    [ConfigurationKeyName("hardware-acceleration")]
    public bool? OverrideHardwareAcceleration { get; init; }

    [ConfigurationKeyName("graphics-backend")]
    public string? OverrideGraphicsBackend { get; init; }

    [ConfigurationKeyName("hide-cursor")]
    public string? OverrideHideCursor { get; init; }

    [ConfigurationKeyName("root-data-dir")]
    public string? BaseDirPathArg { get; init; }

    [ConfigurationKeyName("profile")]
    public string? Profile { get; init; }

    [ConfigurationKeyName("path")]
    public string? LaunchPathArg { get; init; }

    [ConfigurationKeyName("application-id")]
    public string? LaunchApplicationId { get; init; }

    [ConfigurationKeyName("fullscreen")]
    public bool StartFullscreenArg { get; init; }

    [ConfigurationKeyName("config")]
    public string? OverrideConfigFile { get; init; }
}