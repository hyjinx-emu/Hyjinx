using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;

namespace Hyjinx.Logging.File;

internal sealed class SimpleFileFormatter : Formatter<SimpleFileFormatterOptions> 
{
    internal SimpleFileFormatter(IOptionsMonitor<SimpleFileFormatterOptions> options) 
        : base(FileFormatterNames.Simple, options) { }

    protected override void WriteCore(string message, LogLevel level, TextWriter textWriter)
    {
        textWriter.Write(message);
    }
}
