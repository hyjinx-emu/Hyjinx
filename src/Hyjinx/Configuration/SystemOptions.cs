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
}