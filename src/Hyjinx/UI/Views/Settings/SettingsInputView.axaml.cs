using Avalonia.Controls;

namespace Hyjinx.Ava.UI.Views.Settings
{
    public partial class SettingsInputView : UserControl
    {
        public SettingsInputView()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            InputView.Dispose();
        }
    }
}
