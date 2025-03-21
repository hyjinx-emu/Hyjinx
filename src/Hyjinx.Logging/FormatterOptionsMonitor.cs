// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Logging.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Hyjinx.Logging;

internal sealed class FormatterOptionsMonitor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions> :
    IOptionsMonitor<TOptions>
    where TOptions : FormatterOptions
{
    private readonly TOptions _options;

    public FormatterOptionsMonitor(TOptions options)
    {
        _options = options;
    }

    public TOptions Get(string? name) => _options;

    public IDisposable? OnChange(Action<TOptions, string> listener)
    {
        return null;
    }

    public TOptions CurrentValue => _options;
}
