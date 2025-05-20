// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Hyjinx.Logging.File;

/// <summary>
/// Provides extension methods for the <see cref="ILoggingBuilder"/> and <see cref="ILoggerProviderConfiguration{FileLoggerProvider}"/> classes.
/// </summary>
[UnsupportedOSPlatform("browser")]
public static class FileLoggerExtensions
{
    internal const string RequiresDynamicCodeMessage = "Binding TOptions to configuration values may require generating dynamic code at runtime.";
    internal const string TrimmingRequiresUnreferencedCodeMessage = "TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.";

    /// <summary>
    /// Adds a file logger named 'File' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.AddFileFormatter<SimpleFileFormatter, SimpleFileFormatterOptions, SimpleFileFormatterConfigureOptions>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<FileLoggerOptions>, FileLoggerConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<FileLoggerOptions>, LoggerProviderOptionsChangeTokenSource<FileLoggerOptions, FileLoggerProvider>>());

        return builder;
    }

    /// <summary>
    /// Adds a file logger named 'File' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <param name="configure">A delegate to configure the <see cref="FileLogger"/>.</param>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddFile();
        builder.Services.Configure(configure);

        return builder;
    }

    /// <summary>
    /// Add the default file log formatter named 'simple' to the factory with default properties.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    public static ILoggingBuilder AddSimpleFile(this ILoggingBuilder builder) =>
        builder.AddFormatterWithName(FileFormatterNames.Simple);

    /// <summary>
    /// Add and configure a file log formatter named 'simple' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <param name="configure">A delegate to configure the <see cref="FileLogger"/> options for the built-in default log formatter.</param>
    public static ILoggingBuilder AddSimpleFile(this ILoggingBuilder builder, Action<SimpleFileFormatterOptions> configure)
    {
        return builder.AddFileWithFormatter(FileFormatterNames.Simple, configure);
    }

    internal static ILoggingBuilder AddFileWithFormatter<TOptions>(this ILoggingBuilder builder, string name, Action<TOptions> configure)
        where TOptions : FormatterOptions
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddFormatterWithName(name);
        builder.Services.Configure(configure);

        return builder;
    }

    private static ILoggingBuilder AddFormatterWithName(this ILoggingBuilder builder, string name) =>
        builder.AddFile(options => options.FormatterName = name);

    /// <summary>
    /// Adds a custom file logger formatter 'TFormatter' to be configured with options 'TOptions'.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ILoggingBuilder AddFileFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFormatter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(this ILoggingBuilder builder)
        where TOptions : FormatterOptions
        where TFormatter : class, IFormatter
    {
        return AddFileFormatter<TFormatter, TOptions, FileLoggerFormatterConfigureOptions<TFormatter, TOptions>>(builder);
    }

    private static ILoggingBuilder AddFileFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFormatter, TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TConfigureOptions>(this ILoggingBuilder builder)
        where TOptions : FormatterOptions
        where TFormatter : class, IFormatter
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IFormatter, TFormatter>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TOptions>, TConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>, FileLoggerFormatterOptionsChangeTokenSource<TFormatter, TOptions>>());

        return builder;
    }

    internal static IConfiguration GetFormatterOptionsSection(this ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration)
    {
        return providerConfiguration.Configuration.GetSection("FormatterOptions");
    }
}

internal sealed class FileLoggerFormatterConfigureOptions<TFormatter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions> : ConfigureFromConfigurationOptions<TOptions>
    where TOptions : FormatterOptions
    where TFormatter : IFormatter
{
    [RequiresDynamicCode(FileLoggerExtensions.RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(FileLoggerExtensions.TrimmingRequiresUnreferencedCodeMessage)]
    public FileLoggerFormatterConfigureOptions(ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration) :
        base(providerConfiguration.GetFormatterOptionsSection())
    {
    }
}

internal sealed class FileLoggerFormatterOptionsChangeTokenSource<TFormatter, TOptions> : ConfigurationChangeTokenSource<TOptions>
    where TOptions : FormatterOptions
    where TFormatter : IFormatter
{
    public FileLoggerFormatterOptionsChangeTokenSource(ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration)
        : base(providerConfiguration.GetFormatterOptionsSection())
    {
    }
}