using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.Controls;

namespace Hyjinx.Ava.UI.Views.User
{
    public partial class UserRecovererView : UserControl
    {
        private NavigationDialogHost _parent;

        public UserRecovererView()
        {
            InitializeComponent();
            AddHandler(Frame.NavigatedToEvent, (s, e) =>
            {
                NavigatedTo(e);
            }, RoutingStrategies.Direct);
        }

        private void NavigatedTo(NavigationEventArgs arg)
        {
            if (Program.PreviewerDetached)
            {
                switch (arg.NavigationMode)
                {
                    case NavigationMode.New:
                        var parent = (NavigationDialogHost)arg.Parameter;

                        _parent = parent;

                        ((ContentDialog)_parent.Parent).Title = $"{LocaleManager.Instance[LocaleKeys.UserProfileWindowTitle]} - {LocaleManager.Instance[LocaleKeys.UserProfilesRecoverHeading]}";

                        break;
                }
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            _parent?.GoBack();
        }

        private void Recover(object sender, RoutedEventArgs e)
        {
            _parent?.RecoverLostAccounts();
        }
    }
}