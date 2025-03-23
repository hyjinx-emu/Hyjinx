// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Hyjinx.Logging.File;

/// <summary>
/// Configures a <see cref="SimpleFileFormatterOptions"/> object from an <see cref="IConfiguration"/>.
/// </summary>
/// <remarks>
/// Doesn't use <see cref="ConfigurationBinder"/> in order to allow <see cref="ConfigurationBinder"/>, and all its dependencies,
/// to be trimmed. This improves app size and startup.
/// </remarks>
internal sealed class SimpleFileFormatterConfigureOptions : IConfigureOptions<SimpleFileFormatterOptions>
{
    private readonly IConfiguration _configuration;

    [UnsupportedOSPlatform("browser")]
    public SimpleFileFormatterConfigureOptions(ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration)
    {
        _configuration = providerConfiguration.GetFormatterOptionsSection();
    }

    public void Configure(SimpleFileFormatterOptions options) => options.Configure(_configuration);
}
