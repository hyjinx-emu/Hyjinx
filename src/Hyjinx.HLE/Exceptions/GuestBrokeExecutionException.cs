using System;

namespace Hyjinx.HLE.Exceptions;

public class GuestBrokeExecutionException : Exception
{
    private const string ExMsg = "The guest program broke execution!";

    public GuestBrokeExecutionException() : base(ExMsg) { }
}