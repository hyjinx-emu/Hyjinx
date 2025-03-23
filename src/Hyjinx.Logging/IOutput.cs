// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.Logging;

internal interface IOutput
{
    Task WriteAsync(string message, CancellationToken cancellationToken);
}
