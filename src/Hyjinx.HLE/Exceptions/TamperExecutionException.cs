using System;

namespace Hyjinx.HLE.Exceptions
{
    public class TamperExecutionException : Exception
    {
        public TamperExecutionException(string message) : base(message) { }
    }
}