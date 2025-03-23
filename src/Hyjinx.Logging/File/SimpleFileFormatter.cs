using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;

namespace Hyjinx.Logging.File;

public sealed class SimpleFileFormatter : Formatter<SimpleFileFormatterOptions> 
{
    public SimpleFileFormatter(IOptionsMonitor<SimpleFileFormatterOptions> options) 
        : base(FileFormatterNames.Simple, options) { }

    protected override void WriteCore(string message, LogLevel level, TextWriter textWriter)
    {
        textWriter.Write(message);
    }
}
