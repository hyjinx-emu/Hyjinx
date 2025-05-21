using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.UI.Common.SystemInfo;

[SupportedOSPlatform("windows")]
partial class WindowsSystemInfo : SystemInfo
{
    private static readonly ILogger<WindowsSystemInfo> _logger = Logger.DefaultLoggerFactory.CreateLogger<WindowsSystemInfo>();

    internal WindowsSystemInfo()
    {
        CpuName = $"{GetCpuidCpuName() ?? GetCpuNameWMI()} ; {LogicalCoreCount} logical"; // WMI is very slow
        (RamTotal, RamAvailable) = GetMemoryStats();
    }

    private static (ulong Total, ulong Available) GetMemoryStats()
    {
        MemoryStatusEx memStatus = new();
        if (GlobalMemoryStatusEx(ref memStatus))
        {
            return (memStatus.TotalPhys, memStatus.AvailPhys); // Bytes
        }

        LogFailedDueToError(_logger, nameof(GlobalMemoryStatusEx), Marshal.GetLastWin32Error());

        return (0, 0);
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "{methodName} failed. Error {errorCode:X}")]
    private static partial void LogFailedDueToError(ILogger logger, string methodName, int errorCode);

    private static string GetCpuNameWMI()
    {
        ManagementObjectCollection cpuObjs = GetWMIObjects("root\\CIMV2", "SELECT * FROM Win32_Processor");

        if (cpuObjs != null)
        {
            foreach (var cpuObj in cpuObjs)
            {
                return cpuObj["Name"].ToString().Trim();
            }
        }

        return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER").Trim();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;

        public MemoryStatusEx()
        {
            Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    private static ManagementObjectCollection GetWMIObjects(string scope, string query)
    {
        try
        {
            return new ManagementObjectSearcher(scope, query).Get();
        }
        catch (PlatformNotSupportedException ex)
        {
            LogWmiNotAvailable(_logger, ex.Message, ex);
        }
        catch (COMException ex)
        {
            LogWmiNotAvailable(_logger, ex.Message, ex);
        }

        return null;
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "WMI is not available: {message}")]
    private static partial void LogWmiNotAvailable(ILogger logger, string message, Exception ex);

}