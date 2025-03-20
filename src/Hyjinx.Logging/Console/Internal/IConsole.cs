// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.Logging.Console.Internal;

internal interface IConsole
{
    Task WriteAsync(string message, CancellationToken cancellationToken);
}
