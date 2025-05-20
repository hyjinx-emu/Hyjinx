// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;

namespace Hyjinx.Logging.Console;

/// <summary>
/// Configures a <see cref="SimpleConsoleFormatterOptions"/> object from an <see cref="IConfiguration"/>.
/// </summary>
/// <remarks>
/// Doesn't use <see cref="ConfigurationBinder"/> in order to allow <see cref="ConfigurationBinder"/>, and all its dependencies,
/// to be trimmed. This improves app size and startup.
/// </remarks>
internal sealed class SimpleConsoleFormatterConfigureOptions : IConfigureOptions<SimpleConsoleFormatterOptions>
{
    private readonly IConfiguration _configuration;

    [UnsupportedOSPlatform("browser")]
    public SimpleConsoleFormatterConfigureOptions(ILoggerProviderConfiguration<ConsoleLoggerProvider> providerConfiguration)
    {
        _configuration = providerConfiguration.GetFormatterOptionsSection();
    }

    public void Configure(SimpleConsoleFormatterOptions options) => options.Configure(_configuration);
}