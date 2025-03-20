// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Hyjinx.Logging.Console.Internal;

internal readonly struct LogMessageEntry
{
    public LogMessageEntry(string message, bool logAsError = false)
    {
        Message = message;
        LogAsError = logAsError;
    }

    public readonly string Message;
    public readonly bool LogAsError;
}
