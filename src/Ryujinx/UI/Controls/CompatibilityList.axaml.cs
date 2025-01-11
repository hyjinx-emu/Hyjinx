using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Ava.UI.Windows;
using System.Threading.Tasks;

namespace Ryujinx.UI.Controls
{
    public partial class CompatibilityList : UserControl
    {
        public static async Task Show()
        {
            var mainWindow = ((Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow);
            
            ContentDialog contentDialog = new()
            {
                PrimaryButtonText = string.Empty,
                SecondaryButtonText = string.Empty,
                CloseButtonText = LocaleManager.Instance[LocaleKeys.SettingsButtonClose],
                Content = new CompatibilityList
                {
                    DataContext = new CompatibilityViewModel(mainWindow!.ViewModel.Applications)
                }
            };

            Style closeButton = new(x => x.Name("CloseButton"));
            closeButton.Setters.Add(new Setter(WidthProperty, 80d));
            
            Style closeButtonParent = new(x => x.Name("CommandSpace"));
            closeButtonParent.Setters.Add(new Setter(HorizontalAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Right));

            contentDialog.Styles.Add(closeButton);
            contentDialog.Styles.Add(closeButtonParent);

            await ContentDialogHelper.ShowAsync(contentDialog);
        }
        
        public CompatibilityList()
        {
            InitializeComponent();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is not CompatibilityViewModel cvm)
                return;

            if (sender is not TextBox searchBox)
                return;
        
            cvm.Search(searchBox.Text);
        }
    }
}

