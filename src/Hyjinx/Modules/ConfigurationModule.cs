using Hyjinx.Ava.Configuration;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Hyjinx.Ava.Modules;

/// <summary>
/// A module for initializing the configuration infrastructure.
/// </summary>
internal class ConfigurationModule : IModule
{
    public static string ConfigurationPath { get; private set; } = null!;

    /// <summary>
    /// Defines the name of the configuration file.
    /// </summary>
    private const string ConfigFileName = "config.json";

    /// <summary>
    /// The configuration manager in use.
    /// </summary>
    private ApplicationConfigurationManager? ConfigurationManager { get; set; }

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

        // Add the user configuration settings.
        ConfigurationManager = new ApplicationConfigurationManager();
        ConfigurationManager.ApplyUserConfig(ConfigurationPath);

        services.AddSingleton<IGameConfigurationManager>(ConfigurationManager);

        var config = ConfigurationManager.Get();
        services.AddSingleton<IConfiguration>(config);
        services.AddOptions<ConfigurationFileFormat>().Bind(config);

        // Add the configuration state initializer.
        services.AddTransient<ISupportInitialization, ConfigurationStateInitializer>();
    }

    /// <summary>
    /// A mechanism to initialize the legacy configuration state object.
    /// </summary>
    /// <param name="opts">The configuration file options.</param>
    [ExcludeFromCodeCoverage]
    internal class ConfigurationStateInitializer(IOptionsMonitor<ConfigurationFileFormat> opts) : ISupportInitialization
    {
        public void Initialize()
        {
            ConfigurationState.Initialize(opts);
        }
    }

    /// <summary>
    /// A manager for handling application configuration files being applied during execution.
    /// </summary>
    internal sealed class ApplicationConfigurationManager : IGameConfigurationManager, IDisposable
    {
        private readonly ConfigurationManager _configManager = new();
        private string? _currentGameConfig;

        ~ApplicationConfigurationManager()
        {
            Dispose(false);
        }

        public void ApplyGameConfig(string applicationId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

            GuardGameConfigMustNotAlreadyBeActive();

            var gameConfigPath = Path.Join(AppDataManager.GamesDirPath, applicationId, ConfigFileName);

            _configManager.AddJsonFile(gameConfigPath, true, true);
            _currentGameConfig = gameConfigPath;
        }

        private void GuardGameConfigMustNotAlreadyBeActive()
        {
            if (_currentGameConfig != null)
            {
                throw new InvalidOperationException("The game configuration has already been set.");
            }
        }

        public void ApplyUserConfig(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            _configManager.AddJsonFile(path, false, true);
        }

        public void ClearGameConfig()
        {
            if (string.IsNullOrEmpty(_currentGameConfig))
            {
                return;
            }

            var existing = _configManager.Sources.OfType<JsonConfigurationSource>().Where(o => string.Equals(_currentGameConfig, o.Path, StringComparison.CurrentCultureIgnoreCase)).SingleOrDefault();
            if (existing != null)
            {
                _configManager.Sources.Remove(existing);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _configManager.Dispose();
            }
        }

        public IConfigurationRoot Get()
        {
            return _configManager;
        }
    }
}