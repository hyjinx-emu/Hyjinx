using Hyjinx.Common.Logging;
using Hyjinx.Common.Memory;
using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.LogManager.Types;
using Hyjinx.Horizon.Sdk.Lm;
using Hyjinx.Horizon.Sdk.Sf;
using Hyjinx.Horizon.Sdk.Sf.Hipc;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hyjinx.Horizon.LogManager.Ipc
{
    partial class LmLogger : ILmLogger
    {
        private const int MessageLengthLimit = 5000;

        private static readonly ILogger<LmLogger> _logger = Logger.DefaultLoggerFactory.CreateLogger<LmLogger>();
        private readonly LogService _log;
        private readonly ulong _pid;

        private LogPacket _logPacket;

        public LmLogger(LogService log, ulong pid)
        {
            _log = log;
            _pid = pid;

            _logPacket = new LogPacket();
        }

        [CmifCommand(0)]
        public Result Log([Buffer(HipcBufferFlags.In | HipcBufferFlags.AutoSelect)] Span<byte> message)
        {
            if (!SetProcessId(message, _pid))
            {
                return Result.Success;
            }

            if (LogImpl(message))
            {
                LogPacketDetails(_logPacket);
                
                _logPacket = new LogPacket();
            }

            return Result.Success;
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceLm, EventName = nameof(LogClass.ServiceLm),
            Message = "{packet}")]
        private partial void LogPacketDetails(LogPacket packet);

        [CmifCommand(1)] // 3.0.0+
        public Result SetDestination(LogDestination destination)
        {
            _log.LogDestination = destination;

            return Result.Success;
        }

        private static bool SetProcessId(Span<byte> message, ulong processId)
        {
            ref LogPacketHeader header = ref MemoryMarshal.Cast<byte, LogPacketHeader>(message)[0];

            uint expectedMessageSize = (uint)Unsafe.SizeOf<LogPacketHeader>() + header.PayloadSize;
            if (expectedMessageSize != (uint)message.Length)
            {
                LogInvalidMessageSize(_logger, expectedMessageSize, message.Length);
                return false;
            }

            header.ProcessId = processId;

            return true;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceLm, EventName = nameof(LogClass.ServiceLm),
            Message = "Invalid message size (expected 0x{expected:X} but got 0x{actual:X}.")]
        private static partial void LogInvalidMessageSize(ILogger logger, uint expected, int actual);
        
        private bool LogImpl(ReadOnlySpan<byte> message)
        {
            SpanReader reader = new(message);

            if (!reader.TryRead(out LogPacketHeader header))
            {
                return true;
            }

            bool isHeadPacket = (header.Flags & LogPacketFlags.IsHead) != 0;
            bool isTailPacket = (header.Flags & LogPacketFlags.IsTail) != 0;

            _logPacket.Severity = header.Severity;

            while (reader.Length > 0)
            {
                if (!TryReadUleb128(ref reader, out int type) || !TryReadUleb128(ref reader, out int size))
                {
                    return true;
                }

                LogDataChunkKey key = (LogDataChunkKey)type;

                switch (key)
                {
                    case LogDataChunkKey.Start:
                        reader.Skip(size);
                        continue;
                    case LogDataChunkKey.Stop:
                        break;
                    case LogDataChunkKey.Line when !reader.TryRead(out _logPacket.Line):
                    case LogDataChunkKey.DropCount when !reader.TryRead(out _logPacket.DropCount):
                    case LogDataChunkKey.Time when !reader.TryRead(out _logPacket.Time):
                        return true;
                    case LogDataChunkKey.Message:
                        {
                            string text = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();

                            if (isHeadPacket && isTailPacket)
                            {
                                _logPacket.Message = text;
                            }
                            else
                            {
                                _logPacket.Message += text;

                                if (_logPacket.Message.Length >= MessageLengthLimit)
                                {
                                    isTailPacket = true;
                                }
                            }

                            break;
                        }
                    case LogDataChunkKey.Filename:
                        _logPacket.Filename = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                        break;
                    case LogDataChunkKey.Function:
                        _logPacket.Function = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                        break;
                    case LogDataChunkKey.Module:
                        _logPacket.Module = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                        break;
                    case LogDataChunkKey.Thread:
                        _logPacket.Thread = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                        break;
                    case LogDataChunkKey.ProgramName:
                        _logPacket.ProgramName = Encoding.UTF8.GetString(reader.GetSpanSafe(size)).TrimEnd();
                        break;
                }
            }

            return isTailPacket;
        }

        private static bool TryReadUleb128(ref SpanReader reader, out int result)
        {
            result = 0;
            int count = 0;
            byte encoded;

            do
            {
                if (!reader.TryRead(out encoded))
                {
                    return false;
                }

                result += (encoded & 0x7F) << (7 * count);

                count++;
            } while ((encoded & 0x80) != 0);

            return true;
        }
    }
}
