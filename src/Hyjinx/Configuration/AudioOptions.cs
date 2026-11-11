using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the audio options.
/// </summary>
[ConfigurationSection(SectionName = SectionName)]
internal class AudioOptions
{
    /// <summary>
    /// Defines the section name.
    /// </summary>
    public const string SectionName = $"{ApplicationOptions.SectionName}:Audio";

    /// <summary>
    /// The selected audio backend.
    /// </summary>
    public AudioBackend Backend { get; set; } = AudioBackend.SDL2;

    /// <summary>
    /// The audio volume.
    /// </summary>
    public float Volume { get; set; } = 1;
}