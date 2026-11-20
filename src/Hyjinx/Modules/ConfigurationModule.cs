using Hyjinx.Ava.Configuration;
using Hyjinx.Common.Configuration;
using Hyjinx.Extensions.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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
    /// <summary>
    /// The configuration manager in use.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static ApplicationConfigurationManager ConfigurationManager { get; } = new();

    [ExcludeFromCodeCoverage]
    public void RegisterServices(IServiceCollection services)
    {
        // Add the user configuration settings.
        ConfigurationManager.ApplyUserConfig();

        services.AddSingleton<IGameConfigurationManager>(ConfigurationManager);

        // Attach the configuration and bind the object.
        var config = ConfigurationManager.GetConfiguration();
        services.AddSingleton(config);
        services.AddOptions<ConfigurationFileFormat>().Bind(config);

        // Add the configuration state initializer.
        services.AddTransient<ISupportInitialization, ConfigurationStateInitializer>();
    }

    /// <summary>
    /// A mechanism to initialize the legacy configuration state object.
    /// </summary>
    /// <param name="opts">The configuration file options.</param>
    internal class ConfigurationStateInitializer(IOptionsMonitor<ConfigurationFileFormat> opts) : ISupportInitialization
    {
        public void Initialize()
        {
            ConfigurationState.Instance.Initialize(opts);
        }
    }

    /// <summary>
    /// A manager for handling application configuration files being applied during execution.
    /// </summary>
    internal sealed class ApplicationConfigurationManager : IGameConfigurationManager, IDisposable
    {
        private readonly ConfigurationManager _configManager;
        private string? _currentGameConfig;

        public ApplicationConfigurationManager() : this(new()) { }

        public ApplicationConfigurationManager(ConfigurationManager configManager)
        {
            _configManager = configManager;
        }

        ~ApplicationConfigurationManager()
        {
            Dispose(false);
        }

        public void ApplyGameConfig(string applicationId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

            GuardGameConfigMustNotAlreadyBeActive();

            string gameConfigFile = AppDataManager.GetGameConfigPath(applicationId);
            if (File.Exists(gameConfigFile))
            {
                _configManager.AddJsonFile(gameConfigFile, false, true);
                _currentGameConfig = gameConfigFile;
            }
        }

        private void GuardGameConfigMustNotAlreadyBeActive()
        {
            if (_currentGameConfig != null)
            {
                throw new InvalidOperationException("The game configuration has already been set.");
            }
        }

        public void ApplyUserConfig(bool autoCreate = true)
        {
            string userConfigFile = AppDataManager.GetUserConfigPath();
            if (!File.Exists(userConfigFile))
            {
                if (!autoCreate)
                {
                    throw new FileNotFoundException($"The user config file '{userConfigFile}' does not exist.", userConfigFile);
                }

                new ConfigurationFileFormat().SaveConfig(userConfigFile);
            }

            _configManager.AddJsonFile(userConfigFile, false, true);
        }

        public void ClearGameConfig()
        {
            if (string.IsNullOrEmpty(_currentGameConfig))
            {
                return;
            }

            var current = new FileInfo(_currentGameConfig);
            var existing = _configManager.Sources.OfType<JsonConfigurationSource>().SingleOrDefault(o =>
                string.Equals(o.Path, current.Name, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(Path.TrimEndingDirectorySeparator(((PhysicalFileProvider)o.FileProvider!)?.Root!), current.Directory!.FullName, StringComparison.CurrentCultureIgnoreCase));

            if (existing != null)
            {
                _configManager.Sources.Remove(existing);
                _currentGameConfig = null;
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
                _currentGameConfig = null;
            }
        }

        public IConfiguration GetConfiguration()
        {
            return _configManager;
        }
    }
}