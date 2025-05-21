using Hyjinx.Graphics.Nvdec.FFmpeg.Native;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace Hyjinx.Graphics.Nvdec.FFmpeg;

unsafe partial class FFmpegContext : IDisposable
{
    private static ILogger<FFmpegContext> _logger = Logger.DefaultLoggerFactory.CreateLogger<FFmpegContext>();

    private unsafe delegate int AVCodec_decode(AVCodecContext* avctx, void* outdata, int* got_frame_ptr, AVPacket* avpkt);

    private readonly AVCodec_decode _decodeFrame;
    private static readonly FFmpegApi.av_log_set_callback_callback _logFunc;
    private readonly AVCodec* _codec;
    private readonly AVPacket* _packet;
    private readonly AVCodecContext* _context;

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.FFmpeg, EventName = nameof(LogClass.FFmpeg),
        Message = "Codec wasn't found. Make sure you have the {codecId} codec present in your FFmpeg installation.")]
    private static partial void LogCodecNotFound(ILogger logger, AVCodecID codecId);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.FFmpeg, EventName = nameof(LogClass.FFmpeg),
        Message = "Codec context couldn't be allocated.")]
    private static partial void LogCodecContextNotAllocated(ILogger logger);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.FFmpeg, EventName = nameof(LogClass.FFmpeg),
        Message = "Codec couldn't be opened.")]
    private static partial void LogCodecNotOpened(ILogger logger);

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.FFmpeg, EventName = nameof(LogClass.FFmpeg),
        Message = "Packet couldn't be allocated.")]
    private static partial void LogPacketNotAllocated(ILogger logger);

    public FFmpegContext(AVCodecID codecId)
    {
        _codec = FFmpegApi.avcodec_find_decoder(codecId);
        if (_codec == null)
        {
            LogCodecNotFound(_logger, codecId);
            return;
        }

        _context = FFmpegApi.avcodec_alloc_context3(_codec);
        if (_context == null)
        {
            LogCodecContextNotAllocated(_logger);
            return;
        }

        if (FFmpegApi.avcodec_open2(_context, _codec, null) != 0)
        {
            LogCodecNotOpened(_logger);
            return;
        }

        _packet = FFmpegApi.av_packet_alloc();
        if (_packet == null)
        {
            LogPacketNotAllocated(_logger);
            return;
        }

        int avCodecRawVersion = FFmpegApi.avcodec_version();
        int avCodecMajorVersion = avCodecRawVersion >> 16;
        int avCodecMinorVersion = (avCodecRawVersion >> 8) & 0xFF;

        // libavcodec 59.24 changed AvCodec to move its private API and also move the codec function to an union.
        if (avCodecMajorVersion > 59 || (avCodecMajorVersion == 59 && avCodecMinorVersion > 24))
        {
            _decodeFrame = Marshal.GetDelegateForFunctionPointer<AVCodec_decode>(((FFCodec<AVCodec>*)_codec)->CodecCallback);
        }
        // libavcodec 59.x changed AvCodec private API layout.
        else if (avCodecMajorVersion == 59)
        {
            _decodeFrame = Marshal.GetDelegateForFunctionPointer<AVCodec_decode>(((FFCodecLegacy<AVCodec501>*)_codec)->Decode);
        }
        // libavcodec 58.x and lower
        else
        {
            _decodeFrame = Marshal.GetDelegateForFunctionPointer<AVCodec_decode>(((FFCodecLegacy<AVCodec>*)_codec)->Decode);
        }
    }

    static FFmpegContext()
    {
        _logFunc = Log;

        // Redirect log output.
        FFmpegApi.av_log_set_level(AVLog.MaxOffset);
        FFmpegApi.av_log_set_callback(_logFunc);
    }

    private static void Log(void* ptr, AVLog level, string format, byte* vl)
    {
        if (level > FFmpegApi.av_log_get_level())
        {
            return;
        }

        int lineSize = 1024;
        byte* lineBuffer = stackalloc byte[lineSize];
        int printPrefix = 1;

        FFmpegApi.av_log_format_line(ptr, level, format, vl, lineBuffer, lineSize, &printPrefix);

        string line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer).Trim();

        switch (level)
        {
            case AVLog.Panic:
            case AVLog.Fatal:
            case AVLog.Error:
                _logger.Log(LogLevel.Error, line);
                break;
            case AVLog.Warning:
                _logger.Log(LogLevel.Warning, line);
                break;
            case AVLog.Info:
                _logger.Log(LogLevel.Information, line);
                break;
            case AVLog.Verbose:
            case AVLog.Debug:
                _logger.Log(LogLevel.Debug, line);
                break;
            case AVLog.Trace:
                _logger.Log(LogLevel.Trace, line);
                break;
        }
    }

    public int DecodeFrame(Surface output, ReadOnlySpan<byte> bitstream)
    {
        FFmpegApi.av_frame_unref(output.Frame);

        int result;
        int gotFrame;

        fixed (byte* ptr = bitstream)
        {
            _packet->Data = ptr;
            _packet->Size = bitstream.Length;
            result = _decodeFrame(_context, output.Frame, &gotFrame, _packet);
        }

        if (gotFrame == 0)
        {
            FFmpegApi.av_frame_unref(output.Frame);

            // If the frame was not delivered, it was probably delayed.
            // Get the next delayed frame by passing a 0 length packet.
            _packet->Data = null;
            _packet->Size = 0;
            result = _decodeFrame(_context, output.Frame, &gotFrame, _packet);

            // We need to set B frames to 0 as we already consumed all delayed frames.
            // This prevents the decoder from trying to return a delayed frame next time.
            _context->HasBFrames = 0;
        }

        FFmpegApi.av_packet_unref(_packet);

        if (gotFrame == 0)
        {
            FFmpegApi.av_frame_unref(output.Frame);

            return -1;
        }

        return result < 0 ? result : 0;
    }

    public void Dispose()
    {
        fixed (AVPacket** ppPacket = &_packet)
        {
            FFmpegApi.av_packet_free(ppPacket);
        }

        _ = FFmpegApi.avcodec_close(_context);

        fixed (AVCodecContext** ppContext = &_context)
        {
            FFmpegApi.avcodec_free_context(ppContext);
        }
    }
}