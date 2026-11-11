using Hyjinx.Ava.Configuration.System;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.HLE.HOS;

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

    /// <summary>
    /// Enables or disables profiled translation cache persistency.
    /// </summary>
    public bool EnablePtc { get; set; } = true;

    /// <summary>
    /// Enables or disables guest Internet access
    /// </summary>
    public bool EnableInternetAccess { get; set; } = false;

    /// <summary>
    /// The selected memory manager mode
    /// </summary>
    public MemoryManagerMode MemoryManagerMode { get; set; } = MemoryManagerMode.HostMappedUnsafe;

    /// <summary>
    /// Expands the RAM amount on the emulated system from 4GiB to 8GiB
    /// </summary>
    public bool ExpandRam { get; set; } = false;

    /// <summary>
    /// Enable or disable ignoring missing services
    /// </summary>
    public bool IgnoreMissingServices { get; set; } = false;
}