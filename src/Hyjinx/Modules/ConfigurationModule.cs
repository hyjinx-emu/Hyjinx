using Hyjinx.Ava.Configuration;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Reflection;

namespace Hyjinx.Ava.Modules;

file static class ServiceCollectionConfigurationExtensions
{
    public static IServiceCollection AddConfigurationSection<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        var attribute = typeof(T).GetCustomAttribute<ConfigurationSectionAttribute>();
        if (attribute == null)
        {
            throw new NotSupportedException($"The type '{nameof(T)}' must have the '{nameof(ConfigurationSectionAttribute)}' applied.");
        }

        services.AddOptions<T>().Bind(configuration.GetSection(attribute.SectionName));
        return services;
    }
}

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
        services.AddOptions<ConfigurationFileFormat>().Bind(config);

        // Register the default options.
        services.AddConfigurationSection<ApplicationOptions>(config);
        services.AddConfigurationSection<AudioOptions>(config);
        services.AddConfigurationSection<LoggingOptions>(config);
        services.AddConfigurationSection<SystemOptions>(config);
        services.AddConfigurationSection<UIOptions>(config);
    }
}