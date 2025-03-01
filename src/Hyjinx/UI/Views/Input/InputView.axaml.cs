using Avalonia.Controls;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.Ava.UI.Models;
using Hyjinx.Ava.UI.ViewModels.Input;

namespace Hyjinx.Ava.UI.Views.Input
{
    public partial class InputView : UserControl
    {
        private bool _dialogOpen;
        private InputViewModel ViewModel { get; set; }

        public InputView()
        {
            DataContext = ViewModel = new InputViewModel(this);

            InitializeComponent();
        }

        public void SaveCurrentProfile()
        {
            ViewModel.Save();
        }

        private async void PlayerIndexBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.IsModified && !_dialogOpen)
            {
                _dialogOpen = true;

                var result = await ContentDialogHelper.CreateConfirmationDialog(
                    LocaleManager.Instance[LocaleKeys.DialogControllerSettingsModifiedConfirmMessage],
                    LocaleManager.Instance[LocaleKeys.DialogControllerSettingsModifiedConfirmSubMessage],
                    LocaleManager.Instance[LocaleKeys.InputDialogYes],
                    LocaleManager.Instance[LocaleKeys.InputDialogNo],
                    LocaleManager.Instance[LocaleKeys.HyjinxConfirm]);

                if (result == UserResult.Yes)
                {
                    ViewModel.Save();
                }

                _dialogOpen = false;

                ViewModel.IsModified = false;

                if (e.AddedItems.Count > 0)
                {
                    var player = (PlayerModel)e.AddedItems[0];
                    ViewModel.PlayerId = player.Id;
                }
            }
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }
    }
}
