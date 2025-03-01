using Gtk;
using Hyjinx.Common;
using Hyjinx.Common.Logging;
using Hyjinx.UI.Common.Configuration;
using System.IO;

namespace Hyjinx.UI.Helper
{
    static class ThemeHelper
    {
        public static void ApplyTheme()
        {
            if (!ConfigurationState.Instance.UI.EnableCustomTheme)
            {
                return;
            }

            if (File.Exists(ConfigurationState.Instance.UI.CustomThemePath) && (Path.GetExtension(ConfigurationState.Instance.UI.CustomThemePath) == ".css"))
            {
                CssProvider cssProvider = new();

                cssProvider.LoadFromPath(ConfigurationState.Instance.UI.CustomThemePath);

                StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
            }
            else
            {
                Logger.Warning?.Print(LogClass.Application, $"The \"custom_theme_path\" section in \"{ReleaseInformation.ConfigName}\" contains an invalid path: \"{ConfigurationState.Instance.UI.CustomThemePath}\".");

                ConfigurationState.Instance.UI.CustomThemePath.Value = "";
                ConfigurationState.Instance.UI.EnableCustomTheme.Value = false;
                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }
    }
}
