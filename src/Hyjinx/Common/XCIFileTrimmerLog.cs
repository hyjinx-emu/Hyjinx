using Hyjinx.Ava.UI.ViewModels;

namespace Hyjinx.Ava.Common
{
    internal class XCIFileTrimmerLog : Ryujinx.Common.Logging.XCIFileTrimmerLog
    {
        private readonly MainWindowViewModel _viewModel;

        public XCIFileTrimmerLog(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void Progress(long current, long total, string text, bool complete)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _viewModel.StatusBarProgressMaximum = (int)(total);
                _viewModel.StatusBarProgressValue = (int)(current);
            });
        }
    }

}
