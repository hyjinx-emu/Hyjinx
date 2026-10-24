using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Hyjinx.Ava.Emulation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Hyjinx.Ava.Modules;

internal class ConfigurationModule : IModule
{
    public static string ConfigurationPath { get; private set; } = null!;

    private class ConfigurationStateInitializer(IOptions<ConfigurationFileFormat> opts) : ISupportInitialization
    {
        public void Initialize()
        {
            ConfigurationState.Instance.Load(opts.Value);
        }
    }

    public void RegisterServices(IServiceCollection services)
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

        services.AddTransient<ISupportInitialization, ConfigurationStateInitializer>();
        services.AddSingleton<IConfiguration>(config);
        services.AddOptions<ConfigurationFileFormat>().Bind(config);
    }
}