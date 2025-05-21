using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.Common.Utilities;
using Hyjinx.Logging.Abstractions;
using Hyjinx.UI.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Hyjinx.Ava.Common;

internal class XCIFileTrimmerLog : XCIFileTrimmer.ILog
{
    private static readonly ILogger<XCIFileTrimmerLog> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<XCIFileTrimmerLog>();

    private readonly MainWindowViewModel _viewModel;

    public XCIFileTrimmerLog(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void Progress(long current, long total, string text, bool complete)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _viewModel.StatusBarProgressMaximum = (int)(total);
            _viewModel.StatusBarProgressValue = (int)(current);
        });
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