
using RTCV.NetCore;
using RTCV.CorruptCore;

using System;
using System.Buffers;
using System.Collections.Generic;
using Hyjinx.Memory;


namespace RTCV.HyjinxVanguard
{
    public static class HyjinxVanguardImplementation
    {
        public static RTCV.Vanguard.VanguardConnector connector = null;
        private static IVirtualMemoryManager _memoryManager = null;

        /// <summary>
        /// Initialize Vanguard connection (call from Hyjinx startup)
        /// </summary>
        public static void StartClient(IVirtualMemoryManager memoryManager, bool attached = false)
        {
            try
            {
                _memoryManager = memoryManager;

                var spec = new NetCoreReceiver();
                spec.Attached = attached;
                spec.MessageReceived += OnMessageReceived;

                connector = new RTCV.Vanguard.VanguardConnector(spec);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting Vanguard: {ex}");
                throw;
            }
        }

        public static void InitializeAttachedMode()
        {
            var mgr = new AttachedMemoryManager();
            StartClient(mgr, true);
        }

        /// <summary>
        /// Replace the current memory manager with the real one and (re)start the connector in non-attached mode.
        /// Call this when the emulator provides a real IVirtualMemoryManager (for example when a game is loaded).
        /// </summary>
        public static void SetMemoryManager(Hyjinx.Memory.IVirtualMemoryManager memoryManager)
        {
            try
            {
                _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));

                // Restart connector in non-attached mode so RTCV can query real domains.
                // We don't attempt to gracefully stop the previous connector - create a new one.
                StartClient(_memoryManager, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set memory manager for Vanguard: {ex}");
                throw;
            }
        }

        private static void OnMessageReceived(object sender, NetCoreEventArgs e)
        {
            try
            {
                var message = e.message;
                var advancedMessage = message as NetCoreAdvancedMessage;

                switch (message.Type)
                {
                    // Handle domain list request
                    case RTCV.NetCore.Commands.Remote.DomainGetDomains:
                        e.setReturnValue(GetMemoryDomains());
                        break;
                    // Handle memory read (RTCV Remote DomainPeekBytes: domainName, address, size)
                    case RTCV.NetCore.Commands.Remote.DomainPeekBytes:
                        {
                            var args = advancedMessage.objectValue as object[];
                            // args: domainName (string), address (ulong), size (int)
                            if (args != null && args.Length >= 3)
                            {
                                ulong address = (ulong)args[1];
                                int size = (int)args[2];
                                e.setReturnValue(ReadMemory(address, size));
                            }
                        }
                        break;

                    // Handle memory write (RTCV Remote DomainPokeBytes: domainName, address, data)
                    case RTCV.NetCore.Commands.Remote.DomainPokeBytes:
                        {
                            var args = advancedMessage.objectValue as object[];
                            // args: domainName (string), address (ulong), data (byte[])
                            if (args != null && args.Length >= 3)
                            {
                                ulong address = (ulong)args[1];
                                byte[] data = args[2] as byte[];
                                if (data != null)
                                {
                                    WriteMemory(address, data);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in message handler: {ex}");
            }
        }

        // Minimal attached-mode memory manager used when Hyjinx starts in -ATTACHED mode.
        // Provides simple, safe stub implementations so Vanguard can query domains without a running emulator.
        internal class AttachedMemoryManager : Hyjinx.Memory.IVirtualMemoryManager
        {
            public bool UsesPrivateAllocations => false;

            public void Map(ulong va, ulong pa, ulong size, Hyjinx.Memory.MemoryMapFlags flags) => throw new NotSupportedException();

            public void MapForeign(ulong va, nuint hostPointer, ulong size) => throw new NotSupportedException();

            public void Unmap(ulong va, ulong size) { }

            public T Read<T>(ulong va) where T : unmanaged => default;

            public void Read(ulong va, Span<byte> data)
            {
                if (data.Length > 0)
                    data.Fill(0);
            }

            public void Write<T>(ulong va, T value) where T : unmanaged { }

            public void Write(ulong va, ReadOnlySpan<byte> data) { }

            public bool WriteWithRedundancyCheck(ulong va, ReadOnlySpan<byte> data) => true;

            public void Fill(ulong va, ulong size, byte value) { }

            public ReadOnlySequence<byte> GetReadOnlySequence(ulong va, int size, bool tracked = false)
            {
                if (size == 0)
                    return ReadOnlySequence<byte>.Empty;
                return new ReadOnlySequence<byte>(new byte[size]);
            }

            public ReadOnlySpan<byte> GetSpan(ulong va, int size, bool tracked = false)
            {
                if (size == 0)
                    return ReadOnlySpan<byte>.Empty;
                return new byte[size];
            }

            public Hyjinx.Memory.IWritableRegion GetWritableRegion(ulong va, int size, bool tracked = false)
            {
                return new SimpleWritableRegion(size);
            }

            public ref T GetRef<T>(ulong va) where T : unmanaged => throw new NotSupportedException();

            public IEnumerable<Hyjinx.Memory.Range.HostMemoryRange> GetHostRegions(ulong va, ulong size) => Array.Empty<Hyjinx.Memory.Range.HostMemoryRange>();

            public IEnumerable<Hyjinx.Memory.Range.MemoryRange> GetPhysicalRegions(ulong va, ulong size) => Array.Empty<Hyjinx.Memory.Range.MemoryRange>();

            public bool IsMapped(ulong va) => false;

            public bool IsRangeMapped(ulong va, ulong size) => false;

            public void SignalMemoryTracking(ulong va, ulong size, bool write, bool precise = false, int? exemptId = null) { }

            public void Reprotect(ulong va, ulong size, Hyjinx.Memory.MemoryPermission protection) { }

            public void TrackingReprotect(ulong va, ulong size, Hyjinx.Memory.MemoryPermission protection, bool guest) { }

            private sealed class SimpleWritableRegion : IWritableRegion
            {
                private readonly Memory<byte> _memory;

                public SimpleWritableRegion(int size)
                {
                    _memory = new byte[Math.Max(0, size)].AsMemory();
                }

                public Memory<byte> Memory => _memory;

                public void Dispose()
                {
                    // nothing to dispose for this simple region
                }
            }
        }

        /// <summary>
        /// Create RTCV-compatible memory domain from Hyjinx memory
        /// </summary>
        private static MemoryDomainProxy[] GetMemoryDomains()
        {
            var domains = new List<MemoryDomainProxy>();

            // Create a single unified domain representing all accessible memory
            var hyjinxDomain = new HyjinxMemoryDomain(_memoryManager);
            domains.Add(new MemoryDomainProxy(hyjinxDomain));

            return domains.ToArray();
        }

        private static byte[] ReadMemory(ulong address, int size)
        {
            byte[] data = new byte[size];
            _memoryManager.Read(address, data);
            return data;
        }

        private static void WriteMemory(ulong address, byte[] data)
        {
            _memoryManager.Write(address, data);
        }
    }

    /// <summary>
    /// Adapter wrapping Hyjinx's IVirtualMemoryManager as RTCV IMemoryDomain
    /// </summary>
    public class HyjinxMemoryDomain : IMemoryDomain
    {
        private readonly IVirtualMemoryManager _memoryManager;

        // Hyjinx typically uses 39-bit address space for Switch, so ~512GB
        private const ulong MAX_ADDRESS = (1UL << 39);

        public string Name => "Main Memory";
        public long Size => (long)MAX_ADDRESS;
        public int WordSize => 1;
        public bool BigEndian => false; // ARM is little-endian

        public HyjinxMemoryDomain(IVirtualMemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
        }

        public byte PeekByte(long addr)
        {
            try
            {
                if (addr < 0 || (ulong)addr >= MAX_ADDRESS)
                    return 0;

                return _memoryManager.Read<byte>((ulong)addr);
            }
            catch
            {
                return 0; // Return 0 for unmapped addresses
            }
        }

        public byte[] PeekBytes(long addr, int range)
        {
            byte[] returnArray = new byte[range];

            try
            {
                if (addr < 0 || (ulong)addr >= MAX_ADDRESS)
                    return returnArray;

                var span = _memoryManager.GetSpan((ulong)addr, range);
                span.CopyTo(returnArray);
            }
            catch
            {
                // Return zeroed array for unmapped regions
            }

            return returnArray;
        }

        public void PokeByte(long addr, byte val)
        {
            try
            {
                if (addr >= 0 && (ulong)addr < MAX_ADDRESS)
                {
                    _memoryManager.Write((ulong)addr, val);
                }
            }
            catch
            {
                // Silently fail for unmapped addresses
            }
        }

        public override string ToString() => Name;
    }
}