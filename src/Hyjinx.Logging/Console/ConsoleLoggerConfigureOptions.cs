// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Hyjinx.Logging.Console;

/// <summary>
/// Configures a <see cref="ConsoleLoggerOptions"/> object from an <see cref="IConfiguration"/>.
/// </summary>
/// <remarks>
/// Doesn't use <see cref="ConfigurationBinder"/> in order to allow <see cref="ConfigurationBinder"/>, and all its dependencies,
/// to be trimmed. This improves app size and startup.
/// </remarks>
internal sealed class ConsoleLoggerConfigureOptions : IConfigureOptions<ConsoleLoggerOptions>
{
    private readonly IConfiguration _configuration;

    [UnsupportedOSPlatform("browser")]
    public ConsoleLoggerConfigureOptions(ILoggerProviderConfiguration<ConsoleLoggerProvider> providerConfiguration)
    {
        _configuration = providerConfiguration.Configuration;
    }

    public void Configure(ConsoleLoggerOptions options) => _configuration.Bind(options);
}
