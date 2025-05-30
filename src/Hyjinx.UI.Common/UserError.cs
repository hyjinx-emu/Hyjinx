namespace Hyjinx.UI.Common;

/// <summary>
/// Represent a common error that could be reported to the user by the emulator.
/// </summary>
public enum UserError
{
    /// <summary>
    /// No error to report.
    /// </summary>
    Success = 0x0,

    /// <summary>
    /// No firmware is installed.
    /// </summary>
    NoFirmware = 0x2,

    /// <summary>
    /// Firmware parsing failed.
    /// </summary>
    /// <remarks>Most likely related to keys.</remarks>
    FirmwareParsingFailed = 0x3,

    /// <summary>
    /// No application was found at the given path.
    /// </summary>
    ApplicationNotFound = 0x4,

    /// <summary>
    /// The firmware is encrypted.
    /// </summary>
    EncryptedFirmwareDetected = 0x5,

    /// <summary>
    /// An unknown error.
    /// </summary>
    Unknown = 0xDEAD,
}