using System;

namespace Hyjinx.HLE.Exceptions;

class InvalidFirmwarePackageException : Exception
{
    public InvalidFirmwarePackageException(string message) : base(message) { }
}