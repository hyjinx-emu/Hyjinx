using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Hyjinx.UI.Common.SystemInfo;

[SupportedOSPlatform("macos")]
partial class MacOSSystemInfo : SystemInfo
{
    internal MacOSSystemInfo()
    {
        if (SysctlByName("kern.osversion", out string buildRevision) != 0)
        {
            buildRevision = "Unknown Build";
        }

        OsDescription = $"macOS {Environment.OSVersion.Version} ({buildRevision}) ({RuntimeInformation.OSArchitecture})";

        string cpuName = GetCpuidCpuName();

        if (cpuName == null && SysctlByName("machdep.cpu.brand_string", out cpuName) != 0)
        {
            cpuName = "Unknown";
        }

        ulong totalRAM = 0;

        if (SysctlByName("hw.memsize", ref totalRAM) != 0) // Bytes
        {
            totalRAM = 0;
        }

        CpuName = $"{cpuName} ; {LogicalCoreCount} logical";
        RamTotal = totalRAM;
        RamAvailable = GetVMInfoAvailableMemory();
    }

    static ulong GetVMInfoAvailableMemory()
    {
        var port = mach_host_self();

        uint pageSize = 0;
        var result = host_page_size(port, ref pageSize);

        if (result != 0)
        {
            LogHostPageSizeError(_logger, result);
            return 0;
        }

        const int Flavor = 4; // HOST_VM_INFO64
        uint count = (uint)(Marshal.SizeOf<VMStatistics64>() / sizeof(int)); // HOST_VM_INFO64_COUNT
        VMStatistics64 stats = new();
        result = host_statistics64(port, Flavor, ref stats, ref count);

        if (result != 0)
        {
            LogHostStatisticsError(_logger, result);
            return 0;
        }

        return (ulong)(stats.FreeCount + stats.InactiveCount) * pageSize;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Failed to query Available RAM. host_page_size() error = {result}")]
    private static partial void LogHostPageSizeError(ILogger logger, int result);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Failed to query Available RAM. host_statistics64() error = {result}")]
    private static partial void LogHostStatisticsError(ILogger logger, int result);

    private const string SystemLibraryName = "libSystem.dylib";

    [LibraryImport(SystemLibraryName, SetLastError = true)]
    private static partial int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string name, IntPtr oldValue, ref ulong oldSize, IntPtr newValue, ulong newValueSize);

    private static int SysctlByName(string name, IntPtr oldValue, ref ulong oldSize)
    {
        if (sysctlbyname(name, oldValue, ref oldSize, IntPtr.Zero, 0) == -1)
        {
            int err = Marshal.GetLastWin32Error();

            LogCannotRetrieveObject(_logger, name, err);
            return err;
        }

        return 0;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "Cannot retrieve '{name}'. Error Code {err}")]
    private static partial void LogCannotRetrieveObject(ILogger logger, string name, int err);

    private static int SysctlByName<T>(string name, ref T oldValue)
    {
        unsafe
        {
            ulong oldValueSize = (ulong)Unsafe.SizeOf<T>();

            return SysctlByName(name, (IntPtr)Unsafe.AsPointer(ref oldValue), ref oldValueSize);
        }
    }

    private static int SysctlByName(string name, out string oldValue)
    {
        oldValue = default;

        ulong strSize = 0;

        int res = SysctlByName(name, IntPtr.Zero, ref strSize);

        if (res == 0)
        {
            byte[] rawData = new byte[strSize];

            unsafe
            {
                fixed (byte* rawDataPtr = rawData)
                {
                    res = SysctlByName(name, (IntPtr)rawDataPtr, ref strSize);
                }

                if (res == 0)
                {
                    oldValue = Encoding.ASCII.GetString(rawData);
                }
            }
        }

        return res;
    }

    [LibraryImport(SystemLibraryName, SetLastError = true)]
    private static partial uint mach_host_self();

    [LibraryImport(SystemLibraryName, SetLastError = true)]
    private static partial int host_page_size(uint host, ref uint out_page_size);

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct VMStatistics64
    {
        public uint FreeCount;
        public uint ActiveCount;
        public uint InactiveCount;
        public uint WireCount;
        public ulong ZeroFillCount;
        public ulong Reactivations;
        public ulong Pageins;
        public ulong Pageouts;
        public ulong Faults;
        public ulong CowFaults;
        public ulong Lookups;
        public ulong Hits;
        public ulong Purges;
        public uint PurgeableCount;
        public uint SpeculativeCount;
        public ulong Decompressions;
        public ulong Compressions;
        public ulong Swapins;
        public ulong Swapouts;
        public uint CompressorPageCount;
        public uint ThrottledCount;
        public uint ExternalPageCount;
        public uint InternalPageCount;
        public ulong TotalUncompressedPagesInCompressor;
    }

    [LibraryImport(SystemLibraryName, SetLastError = true)]
    private static partial int host_statistics64(uint hostPriv, int hostFlavor, ref VMStatistics64 hostInfo64Out, ref uint hostInfo64OutCnt);
}