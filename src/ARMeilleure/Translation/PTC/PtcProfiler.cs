using ARMeilleure.State;
using Hyjinx.Common;
using Hyjinx.Common.Logging;
using Hyjinx.Common.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using static ARMeilleure.Translation.PTC.PtcFormatter;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Timer = System.Timers.Timer;

namespace ARMeilleure.Translation.PTC
{
    partial class PtcProfiler
    {
        private readonly ILogger<PtcProfiler> _logger;
        private const string OuterHeaderMagicString = "Pohd\0\0\0\0";

        private const uint InternalVersion = 5518; //! Not to be incremented manually for each change to the ARMeilleure project.

        private static readonly uint[] _migrateInternalVersions = {
            1866,
        };

        private const int SaveInterval = 30; // Seconds.

        private const CompressionLevel SaveCompressionLevel = CompressionLevel.Fastest;

        private readonly Ptc _ptc;

        private readonly Timer _timer;

        private readonly ulong _outerHeaderMagic;

        private readonly ManualResetEvent _waitEvent;

        private readonly object _lock;

        private bool _disposed;

        private Hash128 _lastHash;

        public Dictionary<ulong, FuncProfile> ProfiledFuncs { get; private set; }

        public bool Enabled { get; private set; }

        public ulong StaticCodeStart { get; set; }
        public ulong StaticCodeSize { get; set; }

        public PtcProfiler(Ptc ptc)
        {
            _logger = Logger.DefaultLoggerFactory.CreateLogger<PtcProfiler>();
            _ptc = ptc;

            _timer = new Timer(SaveInterval * 1000d);
            _timer.Elapsed += PreSave;

            _outerHeaderMagic = BinaryPrimitives.ReadUInt64LittleEndian(EncodingCache.UTF8NoBOM.GetBytes(OuterHeaderMagicString).AsSpan());

            _waitEvent = new ManualResetEvent(true);

            _lock = new object();

            _disposed = false;

            ProfiledFuncs = new Dictionary<ulong, FuncProfile>();

            Enabled = false;
        }

        public void AddEntry(ulong address, ExecutionMode mode, bool highCq)
        {
            if (IsAddressInStaticCodeRange(address))
            {
                Debug.Assert(!highCq);

                lock (_lock)
                {
                    ProfiledFuncs.TryAdd(address, new FuncProfile(mode, highCq: false));
                }
            }
        }

        public void UpdateEntry(ulong address, ExecutionMode mode, bool highCq)
        {
            if (IsAddressInStaticCodeRange(address))
            {
                Debug.Assert(highCq);

                lock (_lock)
                {
                    Debug.Assert(ProfiledFuncs.ContainsKey(address));

                    ProfiledFuncs[address] = new FuncProfile(mode, highCq: true);
                }
            }
        }

        public bool IsAddressInStaticCodeRange(ulong address)
        {
            return address >= StaticCodeStart && address < StaticCodeStart + StaticCodeSize;
        }

        public ConcurrentQueue<(ulong address, FuncProfile funcProfile)> GetProfiledFuncsToTranslate(TranslatorCache<TranslatedFunction> funcs)
        {
            var profiledFuncsToTranslate = new ConcurrentQueue<(ulong address, FuncProfile funcProfile)>();

            foreach (var profiledFunc in ProfiledFuncs)
            {
                if (!funcs.ContainsKey(profiledFunc.Key))
                {
                    profiledFuncsToTranslate.Enqueue((profiledFunc.Key, profiledFunc.Value));
                }
            }

            return profiledFuncsToTranslate;
        }

        public void ClearEntries()
        {
            ProfiledFuncs.Clear();
            ProfiledFuncs.TrimExcess();
        }

        public void PreLoad()
        {
            _lastHash = default;

            string fileNameActual = $"{_ptc.CachePathActual}.info";
            string fileNameBackup = $"{_ptc.CachePathBackup}.info";

            FileInfo fileInfoActual = new(fileNameActual);
            FileInfo fileInfoBackup = new(fileNameBackup);

            if (fileInfoActual.Exists && fileInfoActual.Length != 0L)
            {
                if (!Load(fileNameActual, false))
                {
                    if (fileInfoBackup.Exists && fileInfoBackup.Length != 0L)
                    {
                        Load(fileNameBackup, true);
                    }
                }
            }
            else if (fileInfoBackup.Exists && fileInfoBackup.Length != 0L)
            {
                Load(fileNameBackup, true);
            }
        }

        private bool Load(string fileName, bool isBackup)
        {
            using (FileStream compressedStream = new(fileName, FileMode.Open))
            using (DeflateStream deflateStream = new(compressedStream, CompressionMode.Decompress, true))
            {
                OuterHeader outerHeader = DeserializeStructure<OuterHeader>(compressedStream);

                if (!outerHeader.IsHeaderValid())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.Magic != _outerHeaderMagic)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.InfoFileVersion != InternalVersion && !_migrateInternalVersions.Contains(outerHeader.InfoFileVersion))
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                if (outerHeader.Endianness != Ptc.GetEndianness())
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                using MemoryStream stream = MemoryStreamManager.Shared.GetStream();
                Debug.Assert(stream.Seek(0L, SeekOrigin.Begin) == 0L && stream.Length == 0L);

                try
                {
                    deflateStream.CopyTo(stream);
                }
                catch
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                Debug.Assert(stream.Position == stream.Length);

                stream.Seek(0L, SeekOrigin.Begin);

                Hash128 expectedHash = DeserializeStructure<Hash128>(stream);

                Hash128 actualHash = XXHash128.ComputeHash(GetReadOnlySpan(stream));

                if (actualHash != expectedHash)
                {
                    InvalidateCompressedStream(compressedStream);

                    return false;
                }

                switch (outerHeader.InfoFileVersion)
                {
                    case InternalVersion:
                        ProfiledFuncs = Deserialize(stream);
                        break;
                    case 1866:
                        ProfiledFuncs = Deserialize(stream, (address, profile) => (address + 0x500000UL, profile));
                        break;
                    default:
                        LogNoMigrationPathFound(outerHeader.InfoFileVersion);
                        
                        InvalidateCompressedStream(compressedStream);
                        return false;
                }

                Debug.Assert(stream.Position == stream.Length);

                _lastHash = actualHash;
            }

            long fileSize = new FileInfo(fileName).Length;

            if (isBackup)
            {
                LogLoadedBackupProfilingInfo(fileSize, ProfiledFuncs.Count);
            }
            else
            {
                LogLoadedProfilingInfo(fileSize, ProfiledFuncs.Count);
            }

            return true;
        }

        [LoggerMessage(LogLevel.Information, EventId = (int)LogClass.Ptc, EventName = nameof(LogClass.Ptc),
            Message = "Loaded Profiling Info (size: {fileSize} bytes, profiled functions: {functionsCount}).")]
        protected partial void LogLoadedProfilingInfo(long fileSize, int functionsCount);
        
        [LoggerMessage(LogLevel.Information, EventId = (int)LogClass.Ptc, EventName = nameof(LogClass.Ptc),
            Message = "Loaded Backup Profiling Info (size: {fileSize} bytes, profiled functions: {functionsCount}).")]
        protected partial void LogLoadedBackupProfilingInfo(long fileSize, int functionsCount);

        [LoggerMessage(LogLevel.Error, EventId = (int)LogClass.Ptc, EventName = nameof(LogClass.Ptc),
            Message = "No migration path for version '{fileInfoVersion}'. Discarding cache.")]
        protected partial void LogNoMigrationPathFound(uint fileInfoVersion);

        private static Dictionary<ulong, FuncProfile> Deserialize(Stream stream, Func<ulong, FuncProfile, (ulong, FuncProfile)> migrateEntryFunc = null)
        {
            if (migrateEntryFunc != null)
            {
                return DeserializeAndUpdateDictionary(stream, DeserializeStructure<FuncProfile>, migrateEntryFunc);
            }

            return DeserializeDictionary<ulong, FuncProfile>(stream, DeserializeStructure<FuncProfile>);
        }

        private static ReadOnlySpan<byte> GetReadOnlySpan(MemoryStream memoryStream)
        {
            return new(memoryStream.GetBuffer(), (int)memoryStream.Position, (int)memoryStream.Length - (int)memoryStream.Position);
        }

        private static void InvalidateCompressedStream(FileStream compressedStream)
        {
            compressedStream.SetLength(0L);
        }

        private void PreSave(object source, ElapsedEventArgs e)
        {
            _waitEvent.Reset();

            string fileNameActual = $"{_ptc.CachePathActual}.info";
            string fileNameBackup = $"{_ptc.CachePathBackup}.info";

            FileInfo fileInfoActual = new(fileNameActual);

            if (fileInfoActual.Exists && fileInfoActual.Length != 0L)
            {
                File.Copy(fileNameActual, fileNameBackup, true);
            }

            Save(fileNameActual);

            _waitEvent.Set();
        }

        private void Save(string fileName)
        {
            int profiledFuncsCount;

            OuterHeader outerHeader = new()
            {
                Magic = _outerHeaderMagic,

                InfoFileVersion = InternalVersion,
                Endianness = Ptc.GetEndianness(),
            };

            outerHeader.SetHeaderHash();

            using (MemoryStream stream = MemoryStreamManager.Shared.GetStream())
            {
                Debug.Assert(stream.Seek(0L, SeekOrigin.Begin) == 0L && stream.Length == 0L);

                stream.Seek(Unsafe.SizeOf<Hash128>(), SeekOrigin.Begin);

                lock (_lock)
                {
                    Serialize(stream, ProfiledFuncs);

                    profiledFuncsCount = ProfiledFuncs.Count;
                }

                Debug.Assert(stream.Position == stream.Length);

                stream.Seek(Unsafe.SizeOf<Hash128>(), SeekOrigin.Begin);
                Hash128 hash = XXHash128.ComputeHash(GetReadOnlySpan(stream));

                stream.Seek(0L, SeekOrigin.Begin);
                SerializeStructure(stream, hash);

                if (hash == _lastHash)
                {
                    return;
                }

                using FileStream compressedStream = new(fileName, FileMode.OpenOrCreate);
                using DeflateStream deflateStream = new(compressedStream, SaveCompressionLevel, true);
                try
                {
                    SerializeStructure(compressedStream, outerHeader);

                    stream.WriteTo(deflateStream);

                    _lastHash = hash;
                }
                catch
                {
                    compressedStream.Position = 0L;

                    _lastHash = default;
                }

                if (compressedStream.Position < compressedStream.Length)
                {
                    compressedStream.SetLength(compressedStream.Position);
                }
            }

            long fileSize = new FileInfo(fileName).Length;

            if (fileSize != 0L)
            {
                LogSavedProfileInfo(fileSize, profiledFuncsCount);
            }
        }

        [LoggerMessage(LogLevel.Information, EventId = (int)LogClass.Ptc, EventName = nameof(LogClass.Ptc),
            Message = "Saved Profiling Info (size: {fileSize} bytes, profiled functions: {functionsCount}).")]
        protected partial void LogSavedProfileInfo(long fileSize, int functionsCount);

        private static void Serialize(Stream stream, Dictionary<ulong, FuncProfile> profiledFuncs)
        {
            SerializeDictionary(stream, profiledFuncs, SerializeStructure);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 29*/)]
        private struct OuterHeader
        {
            public ulong Magic;

            public uint InfoFileVersion;

            public bool Endianness;

            public Hash128 HeaderHash;

            public void SetHeaderHash()
            {
                Span<OuterHeader> spanHeader = MemoryMarshal.CreateSpan(ref this, 1);

                HeaderHash = XXHash128.ComputeHash(MemoryMarshal.AsBytes(spanHeader)[..(Unsafe.SizeOf<OuterHeader>() - Unsafe.SizeOf<Hash128>())]);
            }

            public bool IsHeaderValid()
            {
                Span<OuterHeader> spanHeader = MemoryMarshal.CreateSpan(ref this, 1);

                return XXHash128.ComputeHash(MemoryMarshal.AsBytes(spanHeader)[..(Unsafe.SizeOf<OuterHeader>() - Unsafe.SizeOf<Hash128>())]) == HeaderHash;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1/*, Size = 5*/)]
        public struct FuncProfile
        {
            public ExecutionMode Mode;
            public bool HighCq;

            public FuncProfile(ExecutionMode mode, bool highCq)
            {
                Mode = mode;
                HighCq = highCq;
            }
        }

        public void Start()
        {
            if (_ptc.State == PtcState.Enabled ||
                _ptc.State == PtcState.Continuing)
            {
                Enabled = true;

                _timer.Enabled = true;
            }
        }

        public void Stop()
        {
            Enabled = false;

            if (!_disposed)
            {
                _timer.Enabled = false;
            }
        }

        public void Wait()
        {
            _waitEvent.WaitOne();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _timer.Elapsed -= PreSave;
                _timer.Dispose();

                Wait();
                _waitEvent.Dispose();
            }
        }
    }
}
