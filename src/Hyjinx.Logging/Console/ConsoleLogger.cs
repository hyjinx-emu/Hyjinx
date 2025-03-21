// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Hyjinx.Logging.Console;

/// <summary>
/// A logger that writes messages in the console.
/// </summary>
internal sealed class ConsoleLogger : AbstractLogger
{
    internal ConsoleLogger(string name, LoggerProcessor loggerProcessor, Formatter formatter, IExternalScopeProvider? scopeProvider, LoggerOptions options) 
        : base(name, loggerProcessor, formatter, scopeProvider, options) { }
}
