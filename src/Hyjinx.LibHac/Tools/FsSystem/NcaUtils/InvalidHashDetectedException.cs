using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Thrown when an invalid hash has been detected.
/// </summary>
public class InvalidHashDetectedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    public InvalidHashDetectedException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    /// <param name="innerException">The exception which was the cause of this exception.</param>
    public InvalidHashDetectedException(string message, Exception innerException)
        : base(message, innerException) { }
}