using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.UI.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Hyjinx.UI.Common.AutoConfiguration;

public class ConfigurationModule
{
    public static string ConfigurationPath { get; private set; }
    public static bool UseHardwareAcceleration { get; set; }

    public static void Initialize(IServiceCollection services, LaunchOptions launchOptions)
    {
        string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ReleaseInformation.ConfigName);
        string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, ReleaseInformation.ConfigName);

        string? configurationPath = null;
        // Now load the configuration as the other subsystems are now registered
        if (File.Exists(localConfigurationPath))
        {
            configurationPath = localConfigurationPath;
        }
        else if (File.Exists(appDataConfigurationPath))
        {
            configurationPath = appDataConfigurationPath;
        }

        if (!string.IsNullOrEmpty(launchOptions.OverrideConfigFile) && File.Exists(launchOptions.OverrideConfigFile))
        {
            configurationPath = launchOptions.OverrideConfigFile;
        }

        if (configurationPath == null)
        {
            ConfigurationPath = appDataConfigurationPath;
            new ConfigurationFileFormat().SaveConfig(ConfigurationPath);
        }
        else
        {
            ConfigurationPath = configurationPath;
        }

        var config = new ConfigurationBuilder()
            .AddJsonFile(ConfigurationPath, optional: false, reloadOnChange: true)
            .Build();
        
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IOptions<LaunchOptions>>(new OptionsWrapper<LaunchOptions>(launchOptions));
        services.AddOptions<ConfigurationFileFormat>().Bind(config);
    }
}
