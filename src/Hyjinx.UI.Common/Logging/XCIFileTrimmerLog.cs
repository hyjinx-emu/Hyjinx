using Hyjinx.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Hyjinx.Logging.Abstractions
{
    public class XCIFileTrimmerLog : XCIFileTrimmer.ILog
    {
        private readonly ILogger<XCIFileTrimmerLog> _logger = Logger.DefaultLoggerFactory.CreateLogger<XCIFileTrimmerLog>();
        
        public virtual void Progress(long current, long total, string text, bool complete)
        {
        }

        public void Write(XCIFileTrimmer.LogType logType, string text)
        {
            switch (logType)
            {
                case XCIFileTrimmer.LogType.Info:
                    _logger.LogCritical(new EventId((int)LogClass.XCIFileTrimmer, nameof(LogClass.XCIFileTrimmer)), text);
                    break;
                case XCIFileTrimmer.LogType.Warn:
                    _logger.LogWarning(new EventId((int)LogClass.XCIFileTrimmer, nameof(LogClass.XCIFileTrimmer)), text);
                    break;
                case XCIFileTrimmer.LogType.Error:
                    _logger.LogError(new EventId((int)LogClass.XCIFileTrimmer, nameof(LogClass.XCIFileTrimmer)), text);
                    break;
                case XCIFileTrimmer.LogType.Progress:
                    _logger.LogInformation(new EventId((int)LogClass.XCIFileTrimmer, nameof(LogClass.XCIFileTrimmer)), text);
                    break;
            }
        }
    }
}
