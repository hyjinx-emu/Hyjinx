// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace Hyjinx.Logging.Console;

/// <summary>
/// Provides extension methods for the <see cref="ILoggingBuilder"/> and <see cref="ILoggerProviderConfiguration{ConsoleLoggerProvider}"/> classes.
/// </summary>
[UnsupportedOSPlatform("browser")]
public static class ConsoleLoggerExtensions
{
    internal const string RequiresDynamicCodeMessage = "Binding TOptions to configuration values may require generating dynamic code at runtime.";
    internal const string TrimmingRequiresUnreferencedCodeMessage = "TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.";

    /// <summary>
    /// Adds a console logger named 'Console' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    public static ILoggingBuilder AddConsole(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.AddConsoleFormatter<SimpleConsoleFormatter, SimpleConsoleFormatterOptions, SimpleConsoleFormatterConfigureOptions>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ConsoleLoggerOptions>, ConsoleLoggerConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<ConsoleLoggerOptions>, LoggerProviderOptionsChangeTokenSource<ConsoleLoggerOptions, ConsoleLoggerProvider>>());

        return builder;
    }

    /// <summary>
    /// Adds a console logger named 'Console' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <param name="configure">A delegate to configure the <see cref="ConsoleLogger"/>.</param>
    public static ILoggingBuilder AddConsole(this ILoggingBuilder builder, Action<ConsoleLoggerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddConsole();
        builder.Services.Configure(configure);

        return builder;
    }

    /// <summary>
    /// Add the default console log formatter named 'simple' to the factory with default properties.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    public static ILoggingBuilder AddSimpleConsole(this ILoggingBuilder builder) =>
        builder.AddFormatterWithName(ConsoleFormatterNames.Simple);

    /// <summary>
    /// Add and configure a console log formatter named 'simple' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <param name="configure">A delegate to configure the <see cref="ConsoleLogger"/> options for the built-in default log formatter.</param>
    public static ILoggingBuilder AddSimpleConsole(this ILoggingBuilder builder, Action<SimpleConsoleFormatterOptions> configure)
    {
        return builder.AddConsoleWithFormatter(ConsoleFormatterNames.Simple, configure);
    }

    internal static ILoggingBuilder AddConsoleWithFormatter<TOptions>(this ILoggingBuilder builder, string name, Action<TOptions> configure)
        where TOptions : FormatterOptions
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddFormatterWithName(name);
        builder.Services.Configure(configure);

        return builder;
    }

    private static ILoggingBuilder AddFormatterWithName(this ILoggingBuilder builder, string name) =>
        builder.AddConsole(options => options.FormatterName = name);

    /// <summary>
    /// Adds a custom console logger formatter 'TFormatter' to be configured with options 'TOptions'.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ILoggingBuilder AddConsoleFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFormatter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(this ILoggingBuilder builder)
        where TOptions : FormatterOptions
        where TFormatter : class, IFormatter
    {
        return AddConsoleFormatter<TFormatter, TOptions, ConsoleLoggerFormatterConfigureOptions<TFormatter, TOptions>>(builder);
    }

    /// <summary>
    /// Adds a custom console logger formatter 'TFormatter' to be configured with options 'TOptions'.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <param name="configure">A delegate to configure options 'TOptions' for custom formatter 'TFormatter'.</param>
    [RequiresDynamicCode(RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(TrimmingRequiresUnreferencedCodeMessage)]
    public static ILoggingBuilder AddConsoleFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFormatter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(this ILoggingBuilder builder, Action<TOptions> configure)
        where TOptions : FormatterOptions
        where TFormatter : class, IFormatter
    {
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddConsoleFormatter<TFormatter, TOptions>();
        builder.Services.Configure(configure);
        return builder;
    }

    private static ILoggingBuilder AddConsoleFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFormatter, TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TConfigureOptions>(this ILoggingBuilder builder)
        where TOptions : FormatterOptions
        where TFormatter : class, IFormatter
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IFormatter, TFormatter>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TOptions>, TConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>, ConsoleLoggerFormatterOptionsChangeTokenSource<TFormatter, TOptions>>());

        return builder;
    }

    internal static IConfiguration GetFormatterOptionsSection(this ILoggerProviderConfiguration<ConsoleLoggerProvider> providerConfiguration)
    {
        return providerConfiguration.Configuration.GetSection("FormatterOptions");
    }
}

internal sealed class ConsoleLoggerFormatterConfigureOptions<TFormatter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions> : ConfigureFromConfigurationOptions<TOptions>
    where TOptions : FormatterOptions
    where TFormatter : IFormatter
{
    [RequiresDynamicCode(ConsoleLoggerExtensions.RequiresDynamicCodeMessage)]
    [RequiresUnreferencedCode(ConsoleLoggerExtensions.TrimmingRequiresUnreferencedCodeMessage)]
    public ConsoleLoggerFormatterConfigureOptions(ILoggerProviderConfiguration<ConsoleLoggerProvider> providerConfiguration) :
        base(providerConfiguration.GetFormatterOptionsSection())
    { }
}

internal sealed class ConsoleLoggerFormatterOptionsChangeTokenSource<TFormatter, TOptions> : ConfigurationChangeTokenSource<TOptions>
    where TOptions : FormatterOptions
    where TFormatter : IFormatter
{
    public ConsoleLoggerFormatterOptionsChangeTokenSource(ILoggerProviderConfiguration<ConsoleLoggerProvider> providerConfiguration)
        : base(providerConfiguration.GetFormatterOptionsSection()) { }
}