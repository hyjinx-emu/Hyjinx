using OpenTK.Graphics.OpenGL;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hyjinx.Graphics.OpenGL
{
    public partial class Debugger
    {
        private static readonly ILogger<Debugger> _logger = Logger.DefaultLoggerFactory.CreateLogger<Debugger>();
        private static DebugProc _debugCallback;

        private static int _counter;

        public static void Initialize(GraphicsDebugLevel logLevel)
        {
            // Disable everything
            GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, (int[])null, false);

            if (logLevel == GraphicsDebugLevel.None)
            {
                GL.Disable(EnableCap.DebugOutputSynchronous);
                GL.DebugMessageCallback(null, IntPtr.Zero);

                return;
            }

            GL.Enable(EnableCap.DebugOutputSynchronous);

            if (logLevel == GraphicsDebugLevel.Error)
            {
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DebugTypeError, DebugSeverityControl.DontCare, 0, (int[])null, true);
            }
            else if (logLevel == GraphicsDebugLevel.Slowdowns)
            {
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DebugTypeError, DebugSeverityControl.DontCare, 0, (int[])null, true);
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DebugTypePerformance, DebugSeverityControl.DontCare, 0, (int[])null, true);
            }
            else
            {
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, (int[])null, true);
            }

            _counter = 0;
            _debugCallback = GLDebugHandler;

            GL.DebugMessageCallback(_debugCallback, IntPtr.Zero);

            LogOpenGlDebuggingEnabled(_logger);
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
            Message = "OpenGL Debugging is enabled. Performance will be negatively impacted.")]
        private static partial void LogOpenGlDebuggingEnabled(ILogger logger);

        private static void GLDebugHandler(
            DebugSource source,
            DebugType type,
            int id,
            DebugSeverity severity,
            int length,
            IntPtr message,
            IntPtr userParam)
        {
            string msg = Marshal.PtrToStringUTF8(message).Replace('\n', ' ');

            switch (type)
            {
                case DebugType.DebugTypeError:
                    _logger.LogError(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), $"{severity}: {msg}\nCallStack={Environment.StackTrace}", "GLERROR");
                    break;
                case DebugType.DebugTypePerformance:
                    _logger.LogWarning(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), $"{severity}: {msg}", "GLPERF");
                    break;
                case DebugType.DebugTypePushGroup:
                    _logger.LogInformation(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)),$"{{ ({id}) {severity}: {msg}", "GLINFO");
                    break;
                case DebugType.DebugTypePopGroup:
                    _logger.LogInformation(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), $"}} ({id}) {severity}: {msg}", "GLINFO");
                    break;
                default:
                    if (source == DebugSource.DebugSourceApplication)
                    {
                        _logger.LogInformation(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), $"{type} {severity}: {msg}", "GLINFO");
                    }
                    else
                    {
                        _logger.LogError(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), $"{type} {severity}: {msg}", "GLDEBUG");
                    }
                    break;
            }
        }

        // Useful debug helpers
        public static void PushGroup(string dbgMsg)
        {
            int counter = Interlocked.Increment(ref _counter);

            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, counter, dbgMsg.Length, dbgMsg);
        }

        public static void PopGroup()
        {
            GL.PopDebugGroup();
        }

        public static void Print(string dbgMsg, DebugType type = DebugType.DebugTypeMarker, DebugSeverity severity = DebugSeverity.DebugSeverityNotification, int id = 999999)
        {
            GL.DebugMessageInsert(DebugSourceExternal.DebugSourceApplication, type, id, severity, dbgMsg.Length, dbgMsg);
        }
    }
}
