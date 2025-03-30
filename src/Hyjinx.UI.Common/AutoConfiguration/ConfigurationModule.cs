using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Logging.Abstractions;
using Hyjinx.UI.Common.Configuration;
using Hyjinx.UI.Common.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Hyjinx.UI.Common.AutoConfiguration;

public class ConfigurationModule
{
    public static string ConfigurationPath { get; private set; }
    public static bool UseHardwareAcceleration { get; private set; }

    public static void Initialize()
    {
        string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ReleaseInformation.ConfigName);
        string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, ReleaseInformation.ConfigName);

        // Now load the configuration as the other subsystems are now registered
        if (File.Exists(localConfigurationPath))
        {
            ConfigurationPath = localConfigurationPath;
        }
        else if (File.Exists(appDataConfigurationPath))
        {
            ConfigurationPath = appDataConfigurationPath;
        }

        if (!string.IsNullOrEmpty(CommandLineState.OverrideConfigFile) && File.Exists(CommandLineState.OverrideConfigFile))
        {
            ConfigurationPath = CommandLineState.OverrideConfigFile;
        }

        if (ConfigurationPath == null)
        {
            // No configuration, we load the default values and save it to disk
            ConfigurationPath = appDataConfigurationPath;
            
            Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                "No configuration file found. Saving default configuration to: {ConfigurationPath}", ConfigurationPath);

            ConfigurationState.Instance.LoadDefault();
            ConfigurationState.Instance.ToFileFormat().SaveConfig(ConfigurationPath);
        }
        else
        {
            Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                "Loading configuration from: {ConfigurationPath}", ConfigurationPath);

            if (ConfigurationFileFormat.TryLoad(ConfigurationPath, out ConfigurationFileFormat configurationFileFormat))
            {
                ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
            }
            else
            {
                Logger.DefaultLogger.LogWarning(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                    "Failed to load config! Loading the default config instead.\nFailed config location: {path}", ConfigurationPath);

                ConfigurationState.Instance.LoadDefault();
            }
        }

        UseHardwareAcceleration = ConfigurationState.Instance.EnableHardwareAcceleration.Value;

        // Check if graphics backend was overridden
        if (CommandLineState.OverrideGraphicsBackend != null)
        {
            if (CommandLineState.OverrideGraphicsBackend.ToLower() == "opengl")
            {
                ConfigurationState.Instance.Graphics.GraphicsBackend.Value = GraphicsBackend.OpenGl;
            }
            else if (CommandLineState.OverrideGraphicsBackend.ToLower() == "vulkan")
            {
                ConfigurationState.Instance.Graphics.GraphicsBackend.Value = GraphicsBackend.Vulkan;
            }
        }

        // Check if docked mode was overriden.
        if (CommandLineState.OverrideDockedMode.HasValue)
        {
            ConfigurationState.Instance.System.EnableDockedMode.Value = CommandLineState.OverrideDockedMode.Value;
        }

        // Check if HideCursor was overridden.
        if (CommandLineState.OverrideHideCursor is not null)
        {
            ConfigurationState.Instance.HideCursor.Value = CommandLineState.OverrideHideCursor!.ToLower() switch
            {
                "never" => HideCursorMode.Never,
                "onidle" => HideCursorMode.OnIdle,
                "always" => HideCursorMode.Always,
                _ => ConfigurationState.Instance.HideCursor.Value,
            };
        }

        // Check if hardware-acceleration was overridden.
        if (CommandLineState.OverrideHardwareAcceleration != null)
        {
            UseHardwareAcceleration = CommandLineState.OverrideHardwareAcceleration.Value;
        }
    }
}
