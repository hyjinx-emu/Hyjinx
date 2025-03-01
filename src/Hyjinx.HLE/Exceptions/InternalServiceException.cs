using System;

namespace Hyjinx.HLE.Exceptions
{
    class InternalServiceException : Exception
    {
        public InternalServiceException(string message) : base(message) { }
    }
}
