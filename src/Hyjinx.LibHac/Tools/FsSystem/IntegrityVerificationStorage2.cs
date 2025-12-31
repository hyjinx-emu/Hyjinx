using LibHac.Common;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Util;
using System;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A storage which provides integrity verification support of data contained within the data storage.
/// </summary>
/// <remarks>This class uses two independent storages, one containing the verification hashes and another for the actual data to read.</remarks>
public class IntegrityVerificationStorage2 : Storage2
{
    private readonly Validity[]? _sectors;

    /// <summary>
    /// The storage level.
    /// </summary>
    protected int Level { get; }

    /// <summary>
    /// The data storage.
    /// </summary>
    protected IStorage2 DataStorage { get; }

    /// <summary>
    /// The hash storage.
    /// </summary>
    protected IStorage2 HashStorage { get; }

    /// <summary>
    /// The integrity check level.
    /// </summary>
    protected IntegrityCheckLevel IntegrityCheckLevel { get; }

    /// <summary>
    /// The sector size.
    /// </summary>
    protected int SectorSize { get; }

    /// <summary>
    /// Identifies whether partial block hashes are being used by the storage.
    /// </summary>
    /// <remarks>Some storages hash the full sector (even empty data) and others only hash the part of the sector used.</remarks>
    protected bool UsePartialBlockHashes { get; }

    public override long Size => DataStorage.Size;

    private IntegrityVerificationStorage2(int level, IStorage2 dataStorage, bool partialBlockHashes, IStorage2 hashStorage, IntegrityCheckLevel integrityCheckLevel, int sectorSize, Validity[]? sectors)
    {
        Level = level;
        DataStorage = dataStorage;
        UsePartialBlockHashes = partialBlockHashes;
        HashStorage = hashStorage;
        IntegrityCheckLevel = integrityCheckLevel;
        SectorSize = sectorSize;
        _sectors = sectors;
    }

    /// <summary>
    /// Creates an instance.
    /// </summary>
    /// <param name="level">The zero-based level of the storage.</param>
    /// <param name="dataStorage">The data storage whose integrity will be verified.</param>
    /// <param name="usePartialBlockHashes"><c>true</c> enables partial block hashes which if a full block is not used, only the remaining data read is hashed.</param>
    /// <param name="hashStorage">The hash storage containing the integrity verification data for the <paramref name="dataStorage"/> provided.</param>
    /// <param name="integrityCheckLevel">The verification level to use while reading data from the storage.</param>
    /// <param name="offset">The offset of the storage within the stream.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <param name="sectorSize">The size of the sector.</param>
    /// <exception cref="ArgumentException"><paramref name="sectorSize"/> is less than or equal to zero.</exception>
    public static IntegrityVerificationStorage2 Create(int level, IStorage2 dataStorage, bool usePartialBlockHashes, IStorage2 hashStorage, IntegrityCheckLevel integrityCheckLevel, long offset, long length, int sectorSize)
    {
        if (sectorSize <= 0)
        {
            throw new ArgumentException("The value must be greater than zero.", nameof(sectorSize));
        }

        Validity[]? sectors = null;
        if (integrityCheckLevel != IntegrityCheckLevel.None)
        {
            // Only initialize the sector validation checks if enabled to reduce memory consumption.
            var sectorCount = BitUtil.DivideUp(length, sectorSize);

            sectors = new Validity[sectorCount];
        }

        return new IntegrityVerificationStorage2(level,
            dataStorage.Slice2(offset, length), usePartialBlockHashes,
            hashStorage,
            integrityCheckLevel, sectorSize, sectors);
    }

    protected override void ReadCore(long offset, Span<byte> buffer)
    {
        if (IntegrityCheckLevel != IntegrityCheckLevel.None)
        {
            var sectorIndex = (int)(offset / SectorSize);

            var validity = CheckSectorValidity(sectorIndex);
            if (validity == Validity.Invalid && IntegrityCheckLevel == IntegrityCheckLevel.ErrorOnInvalid)
            {
                throw new InvalidSectorDetectedException("The sector was invalid.", Level, sectorIndex);
            }
        }

        DataStorage.Read(offset, buffer);
    }

    /// <summary>
    /// Checks the sector validity for the sector index.
    /// </summary>
    /// <remarks>This method uses a read-through caching mechanism to reduce checks whenever possible.</remarks>
    /// <param name="sectorIndex">The zero-based sector index to check.</param>
    /// <returns>The validity of the sector.</returns>
    protected Validity CheckSectorValidity(int sectorIndex)
    {
        if (_sectors![sectorIndex] != Validity.Unchecked)
        {
            return _sectors[sectorIndex];
        }

        Span<byte> hashBuffer = stackalloc byte[Sha256.DigestSize];

        // Read the expected hash from the file.
        var hashOffset = (long)sectorIndex * Sha256.DigestSize;
        HashStorage.Read(hashOffset, hashBuffer);

        var dataOffset = (long)sectorIndex * SectorSize;
        var bytesRead = (int)Math.Min(SectorSize, Size - dataOffset);

        // Read the entire sector from the file, or however many bytes are remaining.
        using var dataBuffer = new RentedArray2<byte>(SectorSize);
        DataStorage.Read(dataOffset, dataBuffer.Span[..bytesRead]);

        var result = CheckSectorValidityCore(
            bytesRead,
            dataBuffer.Span,
            hashBuffer);

        _sectors[sectorIndex] = result;
        return result;
    }

    private Validity CheckSectorValidityCore(int dataBytesRead, Span<byte> dataBuffer, Span<byte> hashBuffer)
    {
        if (UsePartialBlockHashes)
        {
            return CompareHashes(dataBuffer[..dataBytesRead], hashBuffer);
        }

        if (dataBytesRead < dataBuffer.Length)
        {
            // There are occasions when the data within the sector is less than the full sector size.
            dataBuffer[dataBytesRead..].Clear();
        }

        return CompareHashes(dataBuffer, hashBuffer);
    }

    /// <summary>
    /// Compares hashes for equality.
    /// </summary>
    /// <param name="buffer">The buffer whose data to be hashed.</param>
    /// <param name="expected">The expected hash.</param>
    /// <returns><see cref="Validity.Valid"/> if the hashes match, otherwise <see cref="Validity.Invalid"/> if the hashes do not match.</returns>
    private static Validity CompareHashes(Span<byte> buffer, Span<byte> expected)
    {
        Span<byte> hash = stackalloc byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash(buffer, hash);

        return Utilities.SpansEqual(expected, hash) ?
            Validity.Valid : Validity.Invalid;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DataStorage.Dispose();
            HashStorage.Dispose();
        }

        base.Dispose(disposing);
    }
}