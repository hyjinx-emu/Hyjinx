using Hyjinx.Ava.Configuration;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Hyjinx.Ava.Modules;

internal class ConfigurationModule : IModule
{
    public static string ConfigurationPath { get; private set; } = null!;

    /// <summary>
    /// Defines the name of the configuration file.
    /// </summary>
    private const string ConfigFileName = "config.json";

    private class ConfigurationStateInitializer(IOptions<ConfigurationFileFormat> opts) : ISupportInitialization
    {
        public void Initialize()
        {
            ConfigurationState.Instance.Load(opts.Value);
        }
    }

    public void RegisterServices(IServiceCollection services)
    {
        string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, ConfigFileName);

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

        // Find the internal configuration types.
        var configurationTypes = Assembly.GetExecutingAssembly().GetTypes().Where(o => o is { IsClass: true, IsAbstract: false }
            && o.GetCustomAttribute<ConfigurationSectionAttribute>() != null).ToArray();

        foreach (var configurationType in configurationTypes)
        {
            typeof(ConfigurationModule).GetMethod(nameof(RegisterConfigurationSection), BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(configurationType)
                .Invoke(this, [services, config]);
        }
    }

    private void RegisterConfigurationSection<T>(IServiceCollection services, IConfiguration config) where T : class
    {
        var attribute = typeof(T).GetCustomAttribute<ConfigurationSectionAttribute>();
        if (attribute == null)
        {
            throw new NotSupportedException($"The type '{typeof(T).FullName}' must have the '{nameof(ConfigurationSectionAttribute)}' attribute.");
        }

        services.AddOptions<T>().Bind(config.GetSection(attribute.Path));
    }
}