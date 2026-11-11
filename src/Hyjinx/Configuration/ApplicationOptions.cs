using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the application configuration options.
/// </summary>
[ConfigurationSection(SectionName = SectionName)]
internal class ApplicationOptions
{
    /// <summary>
    /// Defines the section name.
    /// </summary>
    public const string SectionName = "Application";

    /// <summary>
    /// The logging options.
    /// </summary>
    public LoggingOptions Logging { get; set; } = new LoggingOptions();
}