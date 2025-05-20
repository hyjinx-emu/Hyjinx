using System;

namespace Hyjinx.HLE.Exceptions
{
    public class InvalidSystemResourceException : Exception
    {
        public InvalidSystemResourceException(string message) : base(message) { }
    }
}