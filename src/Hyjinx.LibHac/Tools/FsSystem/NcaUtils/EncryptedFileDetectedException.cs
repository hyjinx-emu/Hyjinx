using System;

namespace LibHac.Tools.FsSystem.NcaUtils;

/// <summary>
/// Thrown when an encrypted file has been detected.
/// </summary>
public class EncryptedFileDetectedException : Exception
{
    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    public EncryptedFileDetectedException(string message)
        : base(message) { }
}
