using Hyjinx.Common.Memory;
using Hyjinx.Graphics.Texture;
using Hyjinx.Graphics.Vic.Types;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using static Hyjinx.Graphics.Vic.Image.SurfaceCommon;

namespace Hyjinx.Graphics.Vic.Image;

partial class SurfaceReader
{
    private static readonly ILogger<SurfaceReader> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<SurfaceReader>();

    public static Surface Read(
        ResourceManager rm,
        ref SlotConfig config,
        ref SlotSurfaceConfig surfaceConfig,
        ref Array8<PlaneOffsets> offsets)
    {
        switch (surfaceConfig.SlotPixelFormat)
        {
            case PixelFormat.Y8___V8U8_N420:
                return ReadNv12(rm, ref config, ref surfaceConfig, ref offsets);
        }

        LogUnsupportedPixelFormat(_logger, surfaceConfig.SlotPixelFormat);

        int lw = surfaceConfig.SlotLumaWidth + 1;
        int lh = surfaceConfig.SlotLumaHeight + 1;

        return new Surface(rm.SurfacePool, lw, lh);
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Vic, EventName = nameof(LogClass.Vic),
        Message = "Unsupported pixel format '{format}'.")]
    private static partial void LogUnsupportedPixelFormat(ILogger logger, PixelFormat format);

    private unsafe static Surface ReadNv12(
        ResourceManager rm,
        ref SlotConfig config,
        ref SlotSurfaceConfig surfaceConfig,
        ref Array8<PlaneOffsets> offsets)
    {
        InputSurface input = ReadSurface(rm, ref config, ref surfaceConfig, ref offsets, 1, 2);

        int width = input.Width;
        int height = input.Height;

        int yStride = GetPitch(width, 1);
        int uvStride = GetPitch(input.UvWidth, 2);

        Surface output = new(rm.SurfacePool, width, height);

        if (Sse41.IsSupported)
        {
            Vector128<byte> shufMask = Vector128.Create(
                (byte)0, (byte)2, (byte)3, (byte)1,
                (byte)4, (byte)6, (byte)7, (byte)5,
                (byte)8, (byte)10, (byte)11, (byte)9,
                (byte)12, (byte)14, (byte)15, (byte)13);
            Vector128<short> alphaMask = Vector128.Create(0xff << 24).AsInt16();

            int yStrideGap = yStride - width;
            int uvStrideGap = uvStride - input.UvWidth;

            int widthTrunc = width & ~0xf;

            fixed (Pixel* dstPtr = output.Data)
            {
                Pixel* op = dstPtr;

                fixed (byte* src0Ptr = input.Buffer0, src1Ptr = input.Buffer1)
                {
                    byte* i0p = src0Ptr;

                    for (int y = 0; y < height; y++)
                    {
                        byte* i1p = src1Ptr + (y >> 1) * uvStride;

                        int x = 0;

                        for (; x < widthTrunc; x += 16, i0p += 16, i1p += 16)
                        {
                            Vector128<short> ya0 = Sse41.ConvertToVector128Int16(i0p);
                            Vector128<short> ya1 = Sse41.ConvertToVector128Int16(i0p + 8);

                            Vector128<byte> uv = Sse2.LoadVector128(i1p);

                            Vector128<short> uv0 = Sse2.UnpackLow(uv.AsInt16(), uv.AsInt16());
                            Vector128<short> uv1 = Sse2.UnpackHigh(uv.AsInt16(), uv.AsInt16());

                            Vector128<short> rgba0 = Sse2.UnpackLow(ya0, uv0);
                            Vector128<short> rgba1 = Sse2.UnpackHigh(ya0, uv0);
                            Vector128<short> rgba2 = Sse2.UnpackLow(ya1, uv1);
                            Vector128<short> rgba3 = Sse2.UnpackHigh(ya1, uv1);

                            rgba0 = Ssse3.Shuffle(rgba0.AsByte(), shufMask).AsInt16();
                            rgba1 = Ssse3.Shuffle(rgba1.AsByte(), shufMask).AsInt16();
                            rgba2 = Ssse3.Shuffle(rgba2.AsByte(), shufMask).AsInt16();
                            rgba3 = Ssse3.Shuffle(rgba3.AsByte(), shufMask).AsInt16();

                            rgba0 = Sse2.Or(rgba0, alphaMask);
                            rgba1 = Sse2.Or(rgba1, alphaMask);
                            rgba2 = Sse2.Or(rgba2, alphaMask);
                            rgba3 = Sse2.Or(rgba3, alphaMask);

                            Vector128<short> rgba16_0 = Sse41.ConvertToVector128Int16(rgba0.AsByte());
                            Vector128<short> rgba16_1 = Sse41.ConvertToVector128Int16(HighToLow(rgba0.AsByte()));
                            Vector128<short> rgba16_2 = Sse41.ConvertToVector128Int16(rgba1.AsByte());
                            Vector128<short> rgba16_3 = Sse41.ConvertToVector128Int16(HighToLow(rgba1.AsByte()));
                            Vector128<short> rgba16_4 = Sse41.ConvertToVector128Int16(rgba2.AsByte());
                            Vector128<short> rgba16_5 = Sse41.ConvertToVector128Int16(HighToLow(rgba2.AsByte()));
                            Vector128<short> rgba16_6 = Sse41.ConvertToVector128Int16(rgba3.AsByte());
                            Vector128<short> rgba16_7 = Sse41.ConvertToVector128Int16(HighToLow(rgba3.AsByte()));

                            rgba16_0 = Sse2.ShiftLeftLogical(rgba16_0, 2);
                            rgba16_1 = Sse2.ShiftLeftLogical(rgba16_1, 2);
                            rgba16_2 = Sse2.ShiftLeftLogical(rgba16_2, 2);
                            rgba16_3 = Sse2.ShiftLeftLogical(rgba16_3, 2);
                            rgba16_4 = Sse2.ShiftLeftLogical(rgba16_4, 2);
                            rgba16_5 = Sse2.ShiftLeftLogical(rgba16_5, 2);
                            rgba16_6 = Sse2.ShiftLeftLogical(rgba16_6, 2);
                            rgba16_7 = Sse2.ShiftLeftLogical(rgba16_7, 2);

                            Sse2.Store((short*)(op + (uint)x + 0), rgba16_0);
                            Sse2.Store((short*)(op + (uint)x + 2), rgba16_1);
                            Sse2.Store((short*)(op + (uint)x + 4), rgba16_2);
                            Sse2.Store((short*)(op + (uint)x + 6), rgba16_3);
                            Sse2.Store((short*)(op + (uint)x + 8), rgba16_4);
                            Sse2.Store((short*)(op + (uint)x + 10), rgba16_5);
                            Sse2.Store((short*)(op + (uint)x + 12), rgba16_6);
                            Sse2.Store((short*)(op + (uint)x + 14), rgba16_7);
                        }

                        for (; x < width; x++, i1p += (x & 1) * 2)
                        {
                            Pixel* px = op + (uint)x;

                            px->R = Upsample(*i0p++);
                            px->G = Upsample(*i1p);
                            px->B = Upsample(*(i1p + 1));
                            px->A = 0x3ff;
                        }

                        op += width;
                        i0p += yStrideGap;
                        i1p += uvStrideGap;
                    }
                }
            }
        }
        else if (AdvSimd.Arm64.IsSupported)
        {
            Vector128<int> alphaMask = Vector128.Create(0xffu << 24).AsInt32();

            int yStrideGap = yStride - width;
            int uvStrideGap = uvStride - input.UvWidth;

            int widthTrunc = width & ~0xf;

            fixed (Pixel* dstPtr = output.Data)
            {
                Pixel* op = dstPtr;

                fixed (byte* src0Ptr = input.Buffer0, src1Ptr = input.Buffer1)
                {
                    byte* i0p = src0Ptr;

                    for (int y = 0; y < height; y++)
                    {
                        byte* i1p = src1Ptr + (y >> 1) * uvStride;

                        int x = 0;

                        for (; x < widthTrunc; x += 16, i0p += 16, i1p += 16)
                        {
                            Vector128<byte> ya = AdvSimd.LoadVector128(i0p);
                            Vector128<byte> uv = AdvSimd.LoadVector128(i1p);

                            Vector128<short> ya0 = AdvSimd.ZeroExtendWideningLower(ya.GetLower()).AsInt16();
                            Vector128<short> ya1 = AdvSimd.ZeroExtendWideningUpper(ya).AsInt16();

                            Vector128<short> uv0 = AdvSimd.Arm64.ZipLow(uv.AsInt16(), uv.AsInt16());
                            Vector128<short> uv1 = AdvSimd.Arm64.ZipHigh(uv.AsInt16(), uv.AsInt16());

                            ya0 = AdvSimd.ShiftLeftLogical(ya0, 8);
                            ya1 = AdvSimd.ShiftLeftLogical(ya1, 8);

                            Vector128<short> rgba0 = AdvSimd.Arm64.ZipLow(ya0, uv0);
                            Vector128<short> rgba1 = AdvSimd.Arm64.ZipHigh(ya0, uv0);
                            Vector128<short> rgba2 = AdvSimd.Arm64.ZipLow(ya1, uv1);
                            Vector128<short> rgba3 = AdvSimd.Arm64.ZipHigh(ya1, uv1);

                            rgba0 = AdvSimd.ShiftRightLogicalAdd(alphaMask, rgba0.AsInt32(), 8).AsInt16();
                            rgba1 = AdvSimd.ShiftRightLogicalAdd(alphaMask, rgba1.AsInt32(), 8).AsInt16();
                            rgba2 = AdvSimd.ShiftRightLogicalAdd(alphaMask, rgba2.AsInt32(), 8).AsInt16();
                            rgba3 = AdvSimd.ShiftRightLogicalAdd(alphaMask, rgba3.AsInt32(), 8).AsInt16();

                            Vector128<short> rgba16_0 = AdvSimd.ZeroExtendWideningLower(rgba0.AsByte().GetLower()).AsInt16();
                            Vector128<short> rgba16_1 = AdvSimd.ZeroExtendWideningUpper(rgba0.AsByte()).AsInt16();
                            Vector128<short> rgba16_2 = AdvSimd.ZeroExtendWideningLower(rgba1.AsByte().GetLower()).AsInt16();
                            Vector128<short> rgba16_3 = AdvSimd.ZeroExtendWideningUpper(rgba1.AsByte()).AsInt16();
                            Vector128<short> rgba16_4 = AdvSimd.ZeroExtendWideningLower(rgba2.AsByte().GetLower()).AsInt16();
                            Vector128<short> rgba16_5 = AdvSimd.ZeroExtendWideningUpper(rgba2.AsByte()).AsInt16();
                            Vector128<short> rgba16_6 = AdvSimd.ZeroExtendWideningLower(rgba3.AsByte().GetLower()).AsInt16();
                            Vector128<short> rgba16_7 = AdvSimd.ZeroExtendWideningUpper(rgba3.AsByte()).AsInt16();

                            rgba16_0 = AdvSimd.ShiftLeftLogical(rgba16_0, 2);
                            rgba16_1 = AdvSimd.ShiftLeftLogical(rgba16_1, 2);
                            rgba16_2 = AdvSimd.ShiftLeftLogical(rgba16_2, 2);
                            rgba16_3 = AdvSimd.ShiftLeftLogical(rgba16_3, 2);
                            rgba16_4 = AdvSimd.ShiftLeftLogical(rgba16_4, 2);
                            rgba16_5 = AdvSimd.ShiftLeftLogical(rgba16_5, 2);
                            rgba16_6 = AdvSimd.ShiftLeftLogical(rgba16_6, 2);
                            rgba16_7 = AdvSimd.ShiftLeftLogical(rgba16_7, 2);

                            AdvSimd.Store((short*)(op + (uint)x + 0), rgba16_0);
                            AdvSimd.Store((short*)(op + (uint)x + 2), rgba16_1);
                            AdvSimd.Store((short*)(op + (uint)x + 4), rgba16_2);
                            AdvSimd.Store((short*)(op + (uint)x + 6), rgba16_3);
                            AdvSimd.Store((short*)(op + (uint)x + 8), rgba16_4);
                            AdvSimd.Store((short*)(op + (uint)x + 10), rgba16_5);
                            AdvSimd.Store((short*)(op + (uint)x + 12), rgba16_6);
                            AdvSimd.Store((short*)(op + (uint)x + 14), rgba16_7);
                        }

                        for (; x < width; x++, i1p += (x & 1) * 2)
                        {
                            Pixel* px = op + (uint)x;

                            px->R = Upsample(*i0p++);
                            px->G = Upsample(*i1p);
                            px->B = Upsample(*(i1p + 1));
                            px->A = 0x3ff;
                        }

                        op += width;
                        i0p += yStrideGap;
                        i1p += uvStrideGap;
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                int uvBase = (y >> 1) * uvStride;

                for (int x = 0; x < width; x++)
                {
                    output.SetR(x, y, Upsample(input.Buffer0[y * yStride + x]));

                    int uvOffs = uvBase + (x & ~1);

                    output.SetG(x, y, Upsample(input.Buffer1[uvOffs]));
                    output.SetB(x, y, Upsample(input.Buffer1[uvOffs + 1]));
                    output.SetA(x, y, 0x3ff);
                }
            }
        }

        input.Return(rm.BufferPool);

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> HighToLow(Vector128<byte> value)
    {
        return Sse.MoveHighToLow(value.AsSingle(), value.AsSingle()).AsByte();
    }

    private static InputSurface ReadSurface(
        ResourceManager rm,
        ref SlotConfig config,
        ref SlotSurfaceConfig surfaceConfig,
        ref Array8<PlaneOffsets> offsets,
        int bytesPerPixel,
        int planes)
    {
        InputSurface surface = new();

        surface.Initialize();

        int gobBlocksInY = 1 << surfaceConfig.SlotBlkHeight;

        bool linear = surfaceConfig.SlotBlkKind == 0;

        int lw = surfaceConfig.SlotLumaWidth + 1;
        int lh = surfaceConfig.SlotLumaHeight + 1;

        int cw = surfaceConfig.SlotChromaWidth + 1;
        int ch = surfaceConfig.SlotChromaHeight + 1;

        // Interlaced inputs have double the height when deinterlaced.
        int heightShift = config.FrameFormat.IsField() ? 1 : 0;

        surface.Width = lw;
        surface.Height = lh << heightShift;
        surface.UvWidth = cw;
        surface.UvHeight = ch << heightShift;

        if (planes > 0)
        {
            surface.SetBuffer0(ReadBuffer(rm, ref config, ref offsets, linear, 0, lw, lh, bytesPerPixel, gobBlocksInY));
        }

        if (planes > 1)
        {
            surface.SetBuffer1(ReadBuffer(rm, ref config, ref offsets, linear, 1, cw, ch, planes == 2 ? 2 : 1, gobBlocksInY));
        }

        if (planes > 2)
        {
            surface.SetBuffer2(ReadBuffer(rm, ref config, ref offsets, linear, 2, cw, ch, 1, gobBlocksInY));
        }

        return surface;
    }

    private static RentedBuffer ReadBuffer(
        ResourceManager rm,
        scoped ref SlotConfig config,
        scoped ref Array8<PlaneOffsets> offsets,
        bool linear,
        int plane,
        int width,
        int height,
        int bytesPerPixel,
        int gobBlocksInY)
    {
        FrameFormat frameFormat = config.FrameFormat;
        bool isLuma = plane == 0;
        bool isField = frameFormat.IsField();
        bool isTopField = frameFormat.IsTopField(isLuma);
        int stride = GetPitch(width, bytesPerPixel);
        uint offset = GetOffset(ref offsets[0], plane);

        int dstStart = 0;
        int dstStride = stride;

        if (isField)
        {
            dstStart = isTopField ? 0 : stride;
            dstStride = stride * 2;
        }

        RentedBuffer buffer;

        if (linear)
        {
            buffer = ReadBufferLinear(rm, offset, width, height, dstStart, dstStride, bytesPerPixel);
        }
        else
        {
            buffer = ReadBufferBlockLinear(rm, offset, width, height, dstStart, dstStride, bytesPerPixel, gobBlocksInY);
        }

        if (isField || frameFormat.IsInterlaced())
        {
            RentedBuffer prevBuffer = RentedBuffer.Empty;
            RentedBuffer nextBuffer = RentedBuffer.Empty;

            if (config.PrevFieldEnable)
            {
                prevBuffer = ReadBufferNoDeinterlace(rm, ref offsets[1], linear, plane, width, height, bytesPerPixel, gobBlocksInY);
            }

            if (config.NextFieldEnable)
            {
                nextBuffer = ReadBufferNoDeinterlace(rm, ref offsets[2], linear, plane, width, height, bytesPerPixel, gobBlocksInY);
            }

            int w = width * bytesPerPixel;

            switch (config.DeinterlaceMode)
            {
                case DeinterlaceMode.Weave:
                    Scaler.DeinterlaceWeave(buffer.Data, prevBuffer.Data, w, stride, isTopField);
                    break;
                case DeinterlaceMode.BobField:
                    Scaler.DeinterlaceBob(buffer.Data, w, stride, isTopField);
                    break;
                case DeinterlaceMode.Bob:
                    bool isCurrentTop = isLuma ? config.IsEven : config.ChromaEven;
                    Scaler.DeinterlaceBob(buffer.Data, w, stride, isCurrentTop ^ frameFormat.IsInterlacedBottomFirst());
                    break;
                case DeinterlaceMode.NewBob:
                case DeinterlaceMode.Disi1:
                    Scaler.DeinterlaceMotionAdaptive(buffer.Data, prevBuffer.Data, nextBuffer.Data, w, stride, isTopField);
                    break;
                case DeinterlaceMode.WeaveLumaBobFieldChroma:
                    if (isLuma)
                    {
                        Scaler.DeinterlaceWeave(buffer.Data, prevBuffer.Data, w, stride, isTopField);
                    }
                    else
                    {
                        Scaler.DeinterlaceBob(buffer.Data, w, stride, isTopField);
                    }
                    break;
                default:
                    LogUnsupportedDeinterlaceMode(_logger, config.DeinterlaceMode);
                    break;
            }

            prevBuffer.Return(rm.BufferPool);
            nextBuffer.Return(rm.BufferPool);
        }

        return buffer;
    }


    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Vic, EventName = nameof(LogClass.Vic),
        Message = "Unsupported deinterlace mode '{mode}'.")]
    private static partial void LogUnsupportedDeinterlaceMode(ILogger logger, DeinterlaceMode mode);

    private static uint GetOffset(ref PlaneOffsets offsets, int plane)
    {
        return plane switch
        {
            0 => offsets.LumaOffset,
            1 => offsets.ChromaUOffset,
            2 => offsets.ChromaVOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(plane)),
        };
    }

    private static RentedBuffer ReadBufferNoDeinterlace(
        ResourceManager rm,
        ref PlaneOffsets offsets,
        bool linear,
        int plane,
        int width,
        int height,
        int bytesPerPixel,
        int gobBlocksInY)
    {
        int stride = GetPitch(width, bytesPerPixel);
        uint offset = GetOffset(ref offsets, plane);

        if (linear)
        {
            return ReadBufferLinear(rm, offset, width, height, 0, stride, bytesPerPixel);
        }

        return ReadBufferBlockLinear(rm, offset, width, height, 0, stride, bytesPerPixel, gobBlocksInY);
    }

    private static RentedBuffer ReadBufferLinear(
        ResourceManager rm,
        uint offset,
        int width,
        int height,
        int dstStart,
        int dstStride,
        int bytesPerPixel)
    {
        int srcStride = GetPitch(width, bytesPerPixel);
        int inSize = srcStride * height;

        ReadOnlySpan<byte> src = rm.MemoryManager.GetSpan(ExtendOffset(offset), inSize);

        int outSize = dstStride * height;
        int bufferIndex = rm.BufferPool.RentMinimum(outSize, out byte[] buffer);
        Span<byte> dst = buffer;
        dst = dst[..outSize];

        for (int y = 0; y < height; y++)
        {
            src.Slice(y * srcStride, srcStride).CopyTo(dst.Slice(dstStart + y * dstStride, srcStride));
        }

        return new RentedBuffer(dst, bufferIndex);
    }

    private static RentedBuffer ReadBufferBlockLinear(
        ResourceManager rm,
        uint offset,
        int width,
        int height,
        int dstStart,
        int dstStride,
        int bytesPerPixel,
        int gobBlocksInY)
    {
        int inSize = GetBlockLinearSize(width, height, bytesPerPixel, gobBlocksInY);

        ReadOnlySpan<byte> src = rm.MemoryManager.GetSpan(ExtendOffset(offset), inSize);

        int outSize = dstStride * height;
        int bufferIndex = rm.BufferPool.RentMinimum(outSize, out byte[] buffer);
        Span<byte> dst = buffer;
        dst = dst[..outSize];

        LayoutConverter.ConvertBlockLinearToLinear(dst[dstStart..], width, height, dstStride, bytesPerPixel, gobBlocksInY, src);

        return new RentedBuffer(dst, bufferIndex);
    }
}