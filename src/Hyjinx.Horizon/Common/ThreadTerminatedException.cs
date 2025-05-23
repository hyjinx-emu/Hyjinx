using System;

namespace Hyjinx.Horizon.Common;

public class ThreadTerminatedException : Exception
{
    public ThreadTerminatedException() : base("The thread has been terminated.")
    {
    }

    public ThreadTerminatedException(string message) : base(message)
    {
    }

    public ThreadTerminatedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}