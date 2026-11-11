using Hyjinx.Ava.Configuration.UI;
using Hyjinx.Extensions.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the user interface options.
/// </summary>
[ConfigurationSection(SectionName = SectionName)]
internal class UIOptions
{
    /// <summary>
    /// Defines the section name.
    /// </summary>
    public const string SectionName = $"{ApplicationOptions.SectionName}:UI";

    /// <summary>
    /// The main window start-up position, size and state
    /// </summary>
    public MainWindowOptions WindowStartup { get; set; } = new()
    {
        WindowSizeWidth = 1280,
        WindowSizeHeight = 760,
        WindowPositionX = 0,
        WindowPositionY = 0,
        WindowMaximized = false
    };
}