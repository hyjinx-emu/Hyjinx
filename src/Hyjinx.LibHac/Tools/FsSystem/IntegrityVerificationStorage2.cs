using LibHac.Common;
using LibHac.Crypto;
using LibHac.Fs;
using LibHac.Util;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A storage which provides integrity verification support of data contained within the data storage.
/// </summary>
/// <remarks>This class uses two independent storages, one containing the verification hashes and another for the actual data to read.</remarks>
public class IntegrityVerificationStorage2 : AsyncStorage
{
    private readonly int _level;
    private readonly IAsyncStorage _dataStorage;
    private readonly IAsyncStorage _hashStorage;
    private readonly IntegrityCheckLevel _integrityCheckLevel;
    private readonly Validity[]? _sectors;
    private readonly int _sectorSize;

    public override long Position => _dataStorage.Position;

    public override long Length => _dataStorage.Length;

    private IntegrityVerificationStorage2(int level, IAsyncStorage dataStorage, IAsyncStorage hashStorage, IntegrityCheckLevel integrityCheckLevel, int sectorSize, Validity[]? sectors)
    {
        _level = level;
        _dataStorage = dataStorage;
        _hashStorage = hashStorage;
        _integrityCheckLevel = integrityCheckLevel;
        _sectorSize = sectorSize;
        _sectors = sectors;
    }

    /// <summary>
    /// Creates an instance.
    /// </summary>
    /// <param name="level">The zero-based level of the storage.</param>
    /// <param name="dataStorage">The data storage whose integrity will be verified.</param>
    /// <param name="hashStorage">The hash storage containing the integrity verification data for the <paramref name="dataStorage"/> provided.</param>
    /// <param name="integrityCheckLevel">The verification level to use while reading data from the storage.</param>
    /// <param name="offset">The offset of the storage within the stream.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <param name="sectorSize">The size of the sector.</param>
    /// <exception cref="ArgumentException"><paramref name="sectorSize"/> is less than or equal to zero.</exception>
    public static IntegrityVerificationStorage2 Create(int level, IAsyncStorage dataStorage, IAsyncStorage hashStorage, IntegrityCheckLevel integrityCheckLevel, long offset, long length, int sectorSize)
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
        
        var result = new IntegrityVerificationStorage2(level, 
            dataStorage.SliceAsAsync(offset, length), 
            hashStorage,
            integrityCheckLevel, sectorSize, sectors);
        
        result.Seek(0, SeekOrigin.Begin);
        return result;
    }
    
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_integrityCheckLevel != IntegrityCheckLevel.None)
        {
            var sectorIndex = (int)(Position / _sectorSize);
            if (_sectors![sectorIndex] == Validity.Unchecked)
            {
                var validity = await CheckSectorValidityAsync(sectorIndex, cancellationToken);
                _sectors[sectorIndex] = validity;
        
                if (validity == Validity.Invalid && _integrityCheckLevel == IntegrityCheckLevel.ErrorOnInvalid)
                {
                    throw new InvalidSectorDetectedException("The sector was invalid.", _level, sectorIndex);
                }
            }
        }
        
        return await _dataStorage.ReadAsync(buffer, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _dataStorage.Seek(offset, origin);
    }
    
    private async Task<Validity> CheckSectorValidityAsync(int sectorIndex, CancellationToken cancellationToken)
    {
        using var hashBuffer = new RentedArray2<byte>(Sha256.DigestSize);
        var hashOffset = sectorIndex * Sha256.DigestSize;
        
        // Read the expected hash from the file.
        var bytesRead = await _hashStorage.ReadOnceAsync(hashOffset, hashBuffer.Memory, cancellationToken);
        if (bytesRead < Sha256.DigestSize)
        {
            throw new InvalidSectorDetectedException("The expected hash was not the correct size.", _level, sectorIndex);
        }
        
        using var dataBuffer = new RentedArray2<byte>(_sectorSize);
        var dataOffset = sectorIndex * _sectorSize;
        
        // Read the entire sector from the file.
        bytesRead = await _dataStorage.ReadOnceAsync(dataOffset, dataBuffer.Memory, cancellationToken);
        if (bytesRead < dataBuffer.Length)
        {
            // There are occasions when the data within the sector is less than the sector size.
            dataBuffer.Span[bytesRead..].Clear();
        }
        
        return CompareHashes(dataBuffer.Span, hashBuffer.Span);
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

    protected override async ValueTask DisposeAsyncCore()
    {
        await _dataStorage.DisposeAsync();
        await _hashStorage.DisposeAsync();
        
        await base.DisposeAsyncCore();
    }
}