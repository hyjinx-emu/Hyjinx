using Hyjinx.Extensions.Abstractions;
using Hyjinx.Graphics.GAL;
using Hyjinx.Logging.Abstractions;

namespace Hyjinx.Ava.Configuration;

/// <summary>
/// Describes the logging options.
/// </summary>
[ConfigurationSection(SectionName = SectionName)]
internal class LoggingOptions
{
    /// <summary>
    /// Defines the section name.
    /// </summary>
    public const string SectionName = $"{ApplicationOptions.SectionName}:Logging";

    /// <summary>
    /// Enables or disables logging to a file on disk
    /// </summary>
    public bool EnableFileLog { get; set; } = true;

    /// <summary>
    /// Enables printing debug log messages
    /// </summary>
    public bool EnableDebug { get; set; } = false;

    /// <summary>
    /// Enables printing stub log messages
    /// </summary>
    public bool EnableStub { get; set; } = true;

    /// <summary>
    /// Enables printing info log messages
    /// </summary>
    public bool EnableInfo { get; set; } = true;

    /// <summary>
    /// Enables printing warning log messages
    /// </summary>
    public bool EnableWarn { get; set; } = true;

    /// <summary>
    /// Enables printing error log messages
    /// </summary>
    public bool EnableError { get; set; } = true;

    /// <summary>
    /// Enables printing trace log messages
    /// </summary>
    public bool EnableTrace { get; set; } = false;

    /// <summary>
    /// Enables printing guest log messages
    /// </summary>
    public bool EnableGuest { get; set; } = true;

    /// <summary>
    /// Enables printing FS access log messages
    /// </summary>
    public bool EnableFsAccessLog { get; set; } = false;

    /// <summary>
    /// Controls which log messages are written to the log targets
    /// </summary>
    public LogClass[] FilteredClasses { get; set; } = [];

    /// <summary>
    /// Change Graphics API debug log level
    /// </summary>
    public GraphicsDebugLevel GraphicsDebugLevel { get; set; } = GraphicsDebugLevel.None;

    /// <summary>
    /// Enables FS access log output to the console. Possible modes are 0-3
    /// </summary>
    public int FsGlobalAccessLogMode { get; set; } = 0;
}