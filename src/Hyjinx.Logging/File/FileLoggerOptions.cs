namespace Hyjinx.Logging.File;

/// <summary>
/// Options for a <see cref="FileLoggerProvider"/>.
/// </summary>
public class FileLoggerOptions : LoggerOptions
{
    public string? OutputDirectory { get; set; }
}
