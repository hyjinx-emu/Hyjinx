using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Hyjinx.Ava.Common;
using Hyjinx.Ava.Common.Locale;
using Hyjinx.Ava.UI.Helpers;
using Hyjinx.Ava.UI.Windows;
using Hyjinx.Common;
using Hyjinx.Logging.Abstractions;
using Hyjinx.UI.Common.Configuration;
using Hyjinx.UI.Common.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Hyjinx.Ava
{
    public partial class App : Application
    {
        private static readonly ILogger<App> _logger = 
            Logger.DefaultLoggerFactory.CreateLogger<App>();
        
        public override void Initialize()
        {
            Name = $"Hyjinx {Program.Version}";

            AvaloniaXamlLoader.Load(this);

            if (OperatingSystem.IsMacOS())
            {
                Process.Start("/usr/bin/defaults", "write org.hyjinx.Hyjinx ApplePressAndHoldEnabled -bool false");
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();

            if (Program.PreviewerDetached)
            {
                ApplyConfiguredTheme();

                ConfigurationState.Instance.UI.BaseStyle.Event += ThemeChanged_Event;
                ConfigurationState.Instance.UI.CustomThemePath.Event += ThemeChanged_Event;
                ConfigurationState.Instance.UI.EnableCustomTheme.Event += CustomThemeChanged_Event;
            }
        }

        private void CustomThemeChanged_Event(object sender, ReactiveEventArgs<bool> e)
        {
            ApplyConfiguredTheme();
        }

        private void ShowRestartDialog()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await ContentDialogHelper.CreateConfirmationDialog(
                        LocaleManager.Instance[LocaleKeys.DialogThemeRestartMessage],
                        LocaleManager.Instance[LocaleKeys.DialogThemeRestartSubMessage],
                        LocaleManager.Instance[LocaleKeys.InputDialogYes],
                        LocaleManager.Instance[LocaleKeys.InputDialogNo],
                        LocaleManager.Instance[LocaleKeys.DialogRestartRequiredMessage]);

                    if (result == UserResult.Yes)
                    {
                        var path = Environment.ProcessPath;
                        var proc = Process.Start(path, CommandLineState.Arguments);
                        desktop.Shutdown();
                        Environment.Exit(0);
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void ThemeChanged_Event(object sender, ReactiveEventArgs<string> e)
        {
            ApplyConfiguredTheme();
        }

        public void ApplyConfiguredTheme()
        {
            try
            {
                string baseStyle = ConfigurationState.Instance.UI.BaseStyle;

                if (string.IsNullOrWhiteSpace(baseStyle))
                {
                    ConfigurationState.Instance.UI.BaseStyle.Value = "Auto";

                    baseStyle = ConfigurationState.Instance.UI.BaseStyle;
                }

                ThemeVariant systemTheme = DetectSystemTheme();

                ThemeManager.OnThemeChanged();

                RequestedThemeVariant = baseStyle switch
                {
                    "Auto" => systemTheme,
                    "Light" => ThemeVariant.Light,
                    "Dark" => ThemeVariant.Dark,
                    _ => ThemeVariant.Default,
                };
            }
            catch (Exception)
            {
                LogFailedToApplyTheme();
                ShowRestartDialog();
            }
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Failed to Apply Theme. A restart is needed to apply the selected theme.")]
        private partial void LogFailedToApplyTheme();

        /// <summary>
        /// Converts a PlatformThemeVariant value to the corresponding ThemeVariant value.
        /// </summary>
        public static ThemeVariant ConvertThemeVariant(PlatformThemeVariant platformThemeVariant) =>
            platformThemeVariant switch
            {
                PlatformThemeVariant.Dark => ThemeVariant.Dark,
                PlatformThemeVariant.Light => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            };

        public static ThemeVariant DetectSystemTheme()
        {
            if (Application.Current is App app)
            {
                var colorValues = app.PlatformSettings.GetColorValues();

                return ConvertThemeVariant(colorValues.ThemeVariant);
            }

            return ThemeVariant.Default;
        }
    }
}
