using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the application configuration options.
/// </summary>
[ConfigurationSection(Path = SectionPath)]
internal class ApplicationOptions
{
    /// <summary>
    /// Defines the configuration section path.
    /// </summary>
    public const string SectionPath = "Application";

    /// <summary>
    /// The audio options.
    /// </summary>
    public AudioOptions Audio { get; init; } = new();

    /// <summary>
    /// The logging options.
    /// </summary>
    public LoggingOptions Logging { get; init; } = new();

    /// <summary>
    /// The system options.
    /// </summary>
    public SystemOptions System { get; init; } = new();
}
