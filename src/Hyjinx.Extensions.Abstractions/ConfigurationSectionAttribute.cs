using System;

namespace Hyjinx.Extensions.Abstractions;

/// <summary>
/// Identifies a class which describes a configuration section.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ConfigurationSectionAttribute : Attribute
{
    /// <summary>
    /// The path of the configuration section.
    /// </summary>
    public required string Path { get; init; }
}