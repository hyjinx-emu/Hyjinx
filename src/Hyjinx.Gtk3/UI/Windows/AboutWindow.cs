using Gtk;
using Hyjinx.Common.Utilities;
using Hyjinx.UI.Common.Helper;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;

namespace Hyjinx.UI.Windows
{
    public partial class AboutWindow : Window
    {
        public AboutWindow() : base($"Hyjinx {Program.Version} - About")
        {
            InitializeComponent();
        }

        //
        // Events
        //
        private void HyjinxButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://example.com/");
        }

        private void AmiiboApiButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://amiiboapi.com");
        }

        private void PatreonButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://example.com/");
        }

        private void GitHubButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/ryujinx-mirror/Hyjinx");
        }

        private void DiscordButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://discord.gg/xmHPGDfVCa");
        }

        private void TwitterButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://example.com/");
        }

        private void ContributorsButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/ryujinx-mirror/Hyjinx/graphs/contributors?type=a");
        }

        private void ChangelogButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/ryujinx-mirror/Hyjinx/wiki/Changelog#ryujinx-changelog");
        }
    }
}
