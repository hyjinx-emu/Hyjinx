using Hyjinx.Ava.Configuration.System;
using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the system configuration options.
/// </summary>
[ConfigurationSection(SectionName = SectionName)]
internal class SystemOptions
{
    public const string SectionName = $"{ApplicationOptions.SectionName}:System";

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

    /// <summary>
    /// Uses Hypervisor over JIT if available
    /// </summary>
    public bool UseHypervisor { get; set; } = true;
}