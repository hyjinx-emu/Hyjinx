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
    /// The audio options.
    /// </summary>
    public AudioOptions Audio { get; init; } = new();

    /// <summary>
    /// The logging options.
    /// </summary>
    public LoggingOptions Logging { get; init; } = new();

    /// <summary>
    /// The user interface options.
    /// </summary>
    public UIOptions UI { get; init; } = new();
}