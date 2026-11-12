using Hyjinx.Ava.Configuration.System;
using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the system configuration options.
/// </summary>
[ConfigurationSection(Path = SectionPath)]
internal class SystemOptions
{
    /// <summary>
    /// Defines the configuration section path.
    /// </summary>
    public const string SectionPath = $"{ApplicationOptions.SectionPath}:{nameof(ApplicationOptions.System)}";

    /// <summary>
    /// Enables or disables Docked Mode
    /// </summary>
    public bool DockedMode { get; set; } = true;

    /// <summary>
    /// Change System Language
    /// </summary>
    public Language Language { get; set; } = Language.AmericanEnglish;

    /// <summary>
    /// Change System Region
    /// </summary>
    public Region Region { get; set; } = Region.USA;

    /// <summary>
    /// Change System TimeZone
    /// </summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// Change System Time Offset in seconds
    /// </summary>
    public long TimeOffset { get; set; } = 0;
}