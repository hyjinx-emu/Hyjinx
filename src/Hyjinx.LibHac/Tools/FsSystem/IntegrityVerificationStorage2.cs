using LibHac.Common;
using LibHac.Crypto;
using LibHac.Util;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LibHac.Tools.FsSystem;

/// <summary>
/// A storage which provides integrity verification support of data contained within the section.
/// </summary>
public class IntegrityVerificationStorage2 : SubStorage2
{
    private readonly IAsyncStorage _hashStorage;
    private readonly IntegrityCheckLevel _integrityCheckLevel;
    private readonly Validity[]? _sectors;
    private readonly int _sectorSize;
    
    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    /// <param name="dataStorage">The data storage whose integrity will be verified.</param>
    /// <param name="hashStorage">The hash storage containing the integrity verification data for the <paramref name="dataStorage"/> provided.</param>
    /// <param name="integrityCheckLevel">The verification level to use while reading data from the storage.</param>
    /// <param name="offset">The offset of the storage within the stream.</param>
    /// <param name="length">The length of the storage block.</param>
    /// <param name="sectorSize">The size of the sector.</param>
    /// <exception cref="ArgumentException"><paramref name="sectorSize"/> is less than or equal to zero.</exception>
    public IntegrityVerificationStorage2(IAsyncStorage dataStorage, IAsyncStorage hashStorage, IntegrityCheckLevel integrityCheckLevel, long offset, long length, int sectorSize)
        : base(dataStorage, offset, length)
    {
        if (sectorSize <= 0)
        {
            throw new ArgumentException("The value must be greater than zero.", nameof(sectorSize));
        }

        _hashStorage = hashStorage;
        _integrityCheckLevel = integrityCheckLevel;
        _sectorSize = sectorSize;

        if (integrityCheckLevel != IntegrityCheckLevel.None)
        {
            // Only initialize the sector validation checks if enabled to reduce memory consumption.
            var sectorCount = BitUtil.DivideUp(length, sectorSize);
            
            _sectors = new Validity[sectorCount];

            for (var i = 0; i < sectorCount; i++)
            {
                _sectors[i] = Validity.Unchecked;
            }
        }
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
                    throw new InvalidSectorDetectedException("The sector was invalid.", sectorIndex);
                }
            }
        }

        return await base.ReadAsync(buffer, cancellationToken);
    }

    private async ValueTask<Validity> CheckSectorValidityAsync(int sectorIndex, CancellationToken cancellationToken)
    {
        // Grab the starting position so we can move back there if necessary.
        var position = Position;
        
        try
        {
            var buffer = ArrayPool<byte>.Shared.Rent(_sectorSize);
        
            try
            {
                var expectedHash = ArrayPool<byte>.Shared.Rent(Sha256.DigestSize);

                try
                {
                    // Position the hash storage to read the hash sector.
                    _hashStorage.Seek(sectorIndex * Sha256.DigestSize, SeekOrigin.Begin);
                    await _hashStorage.ReadAsync(expectedHash, cancellationToken);
                
                    // Position the stream and read the data sector.
                    Seek(sectorIndex * _sectorSize, SeekOrigin.Begin);
                    await base.ReadAsync(buffer, cancellationToken);
            
                    return VerifySectorCore(buffer, expectedHash);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(expectedHash);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        finally
        {
            // Make sure the stream is repositioned where it started at upon leaving the method.
            Seek(position, SeekOrigin.Begin);
        }
    }

    private static Validity VerifySectorCore(Span<byte> buffer, Span<byte> expected)
    {
        Span<byte> hash = stackalloc byte[Sha256.DigestSize];
        Sha256.GenerateSha256Hash(buffer, hash);

        return Utilities.SpansEqual(expected, hash) ? 
            Validity.Valid : Validity.Invalid;
    }
}