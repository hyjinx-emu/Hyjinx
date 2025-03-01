using Avalonia.Interactivity;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.ViewModels;
using Hyjinx.UI.Common.Models.Amiibo;

namespace Hyjinx.Ava.UI.Windows
{
    public partial class AmiiboWindow : StyleableWindow
    {
        public AmiiboWindow(bool showAll, string lastScannedAmiiboId, string titleId)
        {
            ViewModel = new AmiiboWindowViewModel(this, lastScannedAmiiboId, titleId)
            {
                ShowAllAmiibo = showAll,
            };

            DataContext = ViewModel;

            InitializeComponent();

            Title = $"Hyjinx {Program.Version} - " + LocaleManager.Instance[LocaleKeys.Amiibo];
        }

        public AmiiboWindow()
        {
            ViewModel = new AmiiboWindowViewModel(this, string.Empty, string.Empty);

            DataContext = ViewModel;

            InitializeComponent();

            if (Program.PreviewerDetached)
            {
                Title = $"Hyjinx {Program.Version} - " + LocaleManager.Instance[LocaleKeys.Amiibo];
            }
        }

        public bool IsScanned { get; set; }
        public AmiiboApi ScannedAmiibo { get; set; }
        public AmiiboWindowViewModel ViewModel { get; set; }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.AmiiboSelectedIndex > -1)
            {
                AmiiboApi amiibo = ViewModel.AmiiboList[ViewModel.AmiiboSelectedIndex];
                ScannedAmiibo = amiibo;
                IsScanned = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsScanned = false;

            Close();
        }
    }
}
