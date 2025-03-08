// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.Extensions.Logging.Console.Internal;

/// <summary>
/// For consoles which understand the ANSI escape code sequences to represent color
/// </summary>
internal sealed class AnsiLogConsole : IConsole
{
    private readonly TextWriter _textWriter;

    public AnsiLogConsole(bool stdErr = false)
    {
        _textWriter = stdErr ? System.Console.Error : System.Console.Out;
    }

    public async Task WriteAsync(string message, CancellationToken cancellationToken)
    {
        await _textWriter.WriteAsync(message);
    }
}
