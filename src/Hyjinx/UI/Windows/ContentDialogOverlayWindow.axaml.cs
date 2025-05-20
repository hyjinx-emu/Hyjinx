using Avalonia.Controls;
using Avalonia.Media;

namespace Hyjinx.Ava.UI.Windows
{
    public partial class ContentDialogOverlayWindow : StyleableWindow
    {
        public ContentDialogOverlayWindow()
        {
            InitializeComponent();

            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            WindowStartupLocation = WindowStartupLocation.Manual;
            SystemDecorations = SystemDecorations.None;
            ExtendClientAreaTitleBarHeightHint = 0;
            Background = Brushes.Transparent;
            CanResize = false;
        }
    }
}