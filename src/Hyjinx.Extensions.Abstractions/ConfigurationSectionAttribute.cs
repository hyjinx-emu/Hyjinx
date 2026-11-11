using System;

namespace Hyjinx.Extensions.Abstractions;

/// <summary>
/// Identifies a class as providing configuration options.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ConfigurationSectionAttribute : Attribute
{
    /// <summary>
    /// The name of the section which contains the configuration settings.
    /// </summary>
    public required string SectionName { get; init; }
}