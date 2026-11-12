using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the audio configuration options.
/// </summary>
[ConfigurationSection(Path = SectionPath)]
internal class AudioOptions
{
    /// <summary>
    /// Defines the configuration section path.
    /// </summary>
    public const string SectionPath = $"{ApplicationOptions.SectionPath}:{nameof(ApplicationOptions.Audio)}";

    /// <summary>
    /// The selected audio backend
    /// </summary>
    public AudioBackend AudioBackend { get; set; } = AudioBackend.SDL2;

    /// <summary>
    /// The audio volume
    /// </summary>
    public float AudioVolume { get; set; } = 1;
}
