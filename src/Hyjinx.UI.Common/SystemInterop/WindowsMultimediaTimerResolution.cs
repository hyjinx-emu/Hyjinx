using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.Common.SystemInterop
{
    /// <summary>
    /// Handle Windows Multimedia timer resolution.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class WindowsMultimediaTimerResolution : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TimeCaps
        {
            public uint wPeriodMin;
            public uint wPeriodMax;
        }

        [LibraryImport("winmm.dll", EntryPoint = "timeGetDevCaps", SetLastError = true)]
        private static partial uint TimeGetDevCaps(ref TimeCaps timeCaps, uint sizeTimeCaps);

        [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static partial uint TimeBeginPeriod(uint uMilliseconds);

        [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static partial uint TimeEndPeriod(uint uMilliseconds);

        private readonly ILogger<WindowsMultimediaTimerResolution> _logger = Logger.DefaultLoggerFactory.CreateLogger<WindowsMultimediaTimerResolution>();
        private uint _targetResolutionInMilliseconds;
        private bool _isActive;

        /// <summary>
        /// Create a new <see cref="WindowsMultimediaTimerResolution"/> and activate the given resolution.
        /// </summary>
        /// <param name="targetResolutionInMilliseconds"></param>
        public WindowsMultimediaTimerResolution(uint targetResolutionInMilliseconds)
        {
            _targetResolutionInMilliseconds = targetResolutionInMilliseconds;

            EnsureResolutionSupport();
            Activate();
        }

        private void EnsureResolutionSupport()
        {
            TimeCaps timeCaps = default;

            uint result = TimeGetDevCaps(ref timeCaps, (uint)Unsafe.SizeOf<TimeCaps>());

            if (result != 0)
            {
                LogTimeGetDevCapsFailed(result);
            }
            else
            {
                uint supportedTargetResolutionInMilliseconds = Math.Min(Math.Max(timeCaps.wPeriodMin, _targetResolutionInMilliseconds), timeCaps.wPeriodMax);

                if (supportedTargetResolutionInMilliseconds != _targetResolutionInMilliseconds)
                {
                    LogResolutionNotSupported(supportedTargetResolutionInMilliseconds);

                    _targetResolutionInMilliseconds = supportedTargetResolutionInMilliseconds;
                }
            }
        }

        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "timeGetDevCaps failed with result: {result}")]
        private partial void LogTimeGetDevCapsFailed(uint result);
        
        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Target resolution isn't supported by OS, using closest resolution: {supportedTargetResolutionInMilliseconds}ms")]
        private partial void LogResolutionNotSupported(uint supportedTargetResolutionInMilliseconds);

        private void Activate()
        {
            uint result = TimeBeginPeriod(_targetResolutionInMilliseconds);

            if (result != 0)
            {
                LogTimeBeginPeriodFailed(result);
            }
            else
            {
                _isActive = true;
            }
        }
        
        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "timeBeginPeriod failed with result: {result}")]
        private partial void LogTimeBeginPeriodFailed(uint result);

        private void Disable()
        {
            if (_isActive)
            {
                uint result = TimeEndPeriod(_targetResolutionInMilliseconds);

                if (result != 0)
                {
                    LogTimeEndPeriodFailed(result);
                }
                else
                {
                    _isActive = false;
                }
            }
        }
        
        [LoggerMessage(LogLevel.Critical,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "timeEndPeriod failed with result: {result}")]
        private partial void LogTimeEndPeriodFailed(uint result);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disable();
            }
        }
    }
}
