using System;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// Thrown when an invalid sector is detected.
/// </summary>
public class InvalidSectorDetectedException : Exception
{
    /// <summary>
    /// The index of the sector which was invalid.
    /// </summary>
    public int SectorIndex { get; }
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    /// <param name="sectorIndex">The index of the sector which was invalid.</param>
    public InvalidSectorDetectedException(string message, int sectorIndex)
        : base(message)
    {
        SectorIndex = sectorIndex;
    }
}