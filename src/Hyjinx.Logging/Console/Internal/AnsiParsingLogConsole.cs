// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.Logging.Console.Internal;

[UnsupportedOSPlatform("android")]
[UnsupportedOSPlatform("browser")]
[UnsupportedOSPlatform("ios")]
[UnsupportedOSPlatform("tvos")]
internal sealed class AnsiParsingLogConsole : IOutput
{
    private readonly TextWriter _textWriter;
    private readonly AnsiParser _parser;

    public AnsiParsingLogConsole(bool stdErr = false)
    {
        _textWriter = stdErr ? System.Console.Error : System.Console.Out;
        _parser = new AnsiParser(WriteToConsoleAsync);
    }

    public async Task WriteAsync(string message, CancellationToken cancellationToken)
    {
        await _parser.ParseAsync(message, cancellationToken);
    }

    private static bool SetColor(ConsoleColor? background, ConsoleColor? foreground)
    {
        var backgroundChanged = SetBackgroundColor(background);
        return SetForegroundColor(foreground) || backgroundChanged;
    }

    private static bool SetBackgroundColor(ConsoleColor? background)
    {
        if (background.HasValue)
        {
            System.Console.BackgroundColor = background.Value;
            return true;
        }
        return false;
    }

    private static bool SetForegroundColor(ConsoleColor? foreground)
    {
        if (foreground.HasValue)
        {
            System.Console.ForegroundColor = foreground.Value;
            return true;
        }
        return false;
    }

    private static void ResetColor()
    {
        System.Console.ResetColor();
    }

    private async Task WriteToConsoleAsync(string message, int startIndex, int length, ConsoleColor? background, ConsoleColor? foreground, CancellationToken cancellationToken)
    {
        var span = message.AsMemory(startIndex, length);
        var colorChanged = SetColor(background, foreground);

        await _textWriter.WriteAsync(span, cancellationToken);

        if (colorChanged)
        {
            ResetColor();
        }
    }
}