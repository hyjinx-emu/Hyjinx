using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;

namespace Hyjinx.Logging.File;

/// <summary>
/// Configures a <see cref="FileLoggerOptions"/> object from an <see cref="IConfiguration"/>.
/// </summary>
/// <remarks>
/// Doesn't use <see cref="ConfigurationBinder"/> in order to allow <see cref="ConfigurationBinder"/>, and all its dependencies,
/// to be trimmed. This improves app size and startup.
/// </remarks>
internal sealed class FileLoggerConfigureOptions : IConfigureOptions<FileLoggerOptions>
{
    private readonly IConfiguration _configuration;

    [UnsupportedOSPlatform("browser")]
    public FileLoggerConfigureOptions(ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration)
    {
        _configuration = providerConfiguration.Configuration;
    }

    public void Configure(FileLoggerOptions options) => _configuration.Bind(options);
}

