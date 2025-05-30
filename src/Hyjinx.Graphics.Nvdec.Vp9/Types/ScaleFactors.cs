using Hyjinx.Common.Memory;
using System.Runtime.CompilerServices;
using static Hyjinx.Graphics.Nvdec.Vp9.Dsp.Convolve;
using static Hyjinx.Graphics.Nvdec.Vp9.Dsp.Filter;

namespace Hyjinx.Graphics.Nvdec.Vp9.Types;

internal struct ScaleFactors
{
    private const int RefScaleShift = 14;
    private const int RefNoScale = (1 << RefScaleShift);
    private const int RefInvalidScale = -1;

    private unsafe delegate void ConvolveFn(
        byte* src,
        int srcStride,
        byte* dst,
        int dstStride,
        Array8<short>[] filter,
        int x0Q4,
        int xStepQ4,
        int y0Q4,
        int yStepQ4,
        int w,
        int h);

    private unsafe delegate void HighbdConvolveFn(
        ushort* src,
        int srcStride,
        ushort* dst,
        int dstStride,
        Array8<short>[] filter,
        int x0Q4,
        int xStepQ4,
        int y0Q4,
        int yStepQ4,
        int w,
        int h,
        int bd);

    private static readonly unsafe ConvolveFn[][][] _predictX16Y16 = {
        new[]
        {
            new ConvolveFn[]
            {
                ConvolveCopy,
                ConvolveAvg,
            },
            new ConvolveFn[]
            {
                Convolve8Vert,
                Convolve8AvgVert,
            },
        },
        new[]
        {
            new ConvolveFn[]
            {
                Convolve8Horiz,
                Convolve8AvgHoriz,
            },
            new ConvolveFn[]
            {
                Convolve8,
                Convolve8Avg,
            },
        },
    };

    private static readonly unsafe ConvolveFn[][][] _predictX16 = {
        new[]
        {
            new ConvolveFn[]
            {
                ScaledVert,
                ScaledAvgVert,
            },
            new ConvolveFn[]
            {
                ScaledVert,
                ScaledAvgVert,
            },
        },
        new[]
        {
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
        },
    };

    private static readonly unsafe ConvolveFn[][][] _predictY16 = {
        new[]
        {
            new ConvolveFn[]
            {
                ScaledHoriz,
                ScaledAvgHoriz,
            },
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
        },
        new[]
        {
            new ConvolveFn[]
            {
                ScaledHoriz,
                ScaledAvgHoriz,
            },
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
        },
    };

    private static readonly unsafe ConvolveFn[][][] _predict = {
        new[]
        {
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
        },
        new[]
        {
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
            new ConvolveFn[]
            {
                Scaled2D,
                ScaledAvg2D,
            },
        },
    };

    private static readonly unsafe HighbdConvolveFn[][][] _highbdPredictX16Y16 = {
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolveCopy,
                HighbdConvolveAvg,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8Vert,
                HighbdConvolve8AvgVert,
            },
        },
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8Horiz,
                HighbdConvolve8AvgHoriz,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
        },
    };

    private static readonly unsafe HighbdConvolveFn[][][] _highbdPredictX16 = {
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8Vert,
                HighbdConvolve8AvgVert,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8Vert,
                HighbdConvolve8AvgVert,
            },
        },
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
        },
    };

    private static readonly unsafe HighbdConvolveFn[][][] _highbdPredictY16 = {
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8Horiz,
                HighbdConvolve8AvgHoriz,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
        },
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8Horiz,
                HighbdConvolve8AvgHoriz,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
        },
    };

    private static readonly unsafe HighbdConvolveFn[][][] _highbdPredict = {
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
        },
        new[]
        {
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
            new HighbdConvolveFn[]
            {
                HighbdConvolve8,
                HighbdConvolve8Avg,
            },
        },
    };

    public int XScaleFP; // Horizontal fixed point scale factor
    public int YScaleFP; // Vertical fixed point scale factor
    public int XStepQ4;
    public int YStepQ4;

    public readonly int ScaleValueX(int val)
    {
        return IsScaled() ? ScaledX(val) : val;
    }

    public readonly int ScaleValueY(int val)
    {
        return IsScaled() ? ScaledY(val) : val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void InterPredict(
        int horiz,
        int vert,
        int avg,
        byte* src,
        int srcStride,
        byte* dst,
        int dstStride,
        int subpelX,
        int subpelY,
        int w,
        int h,
        Array8<short>[] kernel,
        int xs,
        int ys)
    {
        if (XStepQ4 == 16)
        {
            if (YStepQ4 == 16)
            {
                // No scaling in either direction.
                _predictX16Y16[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h);
            }
            else
            {
                // No scaling in x direction. Must always scale in the y direction.
                _predictX16[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h);
            }
        }
        else
        {
            if (YStepQ4 == 16)
            {
                // No scaling in the y direction. Must always scale in the x direction.
                _predictY16[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h);
            }
            else
            {
                // Must always scale in both directions.
                _predict[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void HighbdInterPredict(
        int horiz,
        int vert,
        int avg,
        ushort* src,
        int srcStride,
        ushort* dst,
        int dstStride,
        int subpelX,
        int subpelY,
        int w,
        int h,
        Array8<short>[] kernel,
        int xs,
        int ys,
        int bd)
    {
        if (XStepQ4 == 16)
        {
            if (YStepQ4 == 16)
            {
                // No scaling in either direction.
                _highbdPredictX16Y16[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h, bd);
            }
            else
            {
                // No scaling in x direction. Must always scale in the y direction.
                _highbdPredictX16[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h, bd);
            }
        }
        else
        {
            if (YStepQ4 == 16)
            {
                // No scaling in the y direction. Must always scale in the x direction.
                _highbdPredictY16[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h, bd);
            }
            else
            {
                // Must always scale in both directions.
                _highbdPredict[horiz][vert][avg](src, srcStride, dst, dstStride, kernel, subpelX, xs, subpelY, ys, w, h, bd);
            }
        }
    }

    private readonly int ScaledX(int val)
    {
        return (int)((long)val * XScaleFP >> RefScaleShift);
    }

    private readonly int ScaledY(int val)
    {
        return (int)((long)val * YScaleFP >> RefScaleShift);
    }

    private static int GetFixedPointScaleFactor(int otherSize, int thisSize)
    {
        // Calculate scaling factor once for each reference frame
        // and use fixed point scaling factors in decoding and encoding routines.
        // Hardware implementations can calculate scale factor in device driver
        // and use multiplication and shifting on hardware instead of division.
        return (otherSize << RefScaleShift) / thisSize;
    }

    public Mv32 ScaleMv(ref Mv mv, int x, int y)
    {
        int xOffQ4 = ScaledX(x << SubpelBits) & SubpelMask;
        int yOffQ4 = ScaledY(y << SubpelBits) & SubpelMask;
        Mv32 res = new()
        {
            Row = ScaledY(mv.Row) + yOffQ4,
            Col = ScaledX(mv.Col) + xOffQ4,
        };

        return res;
    }

    public readonly bool IsValidScale()
    {
        return XScaleFP != RefInvalidScale && YScaleFP != RefInvalidScale;
    }

    public readonly bool IsScaled()
    {
        return IsValidScale() && (XScaleFP != RefNoScale || YScaleFP != RefNoScale);
    }

    public static bool ValidRefFrameSize(int refWidth, int refHeight, int thisWidth, int thisHeight)
    {
        return 2 * thisWidth >= refWidth &&
               2 * thisHeight >= refHeight &&
               thisWidth <= 16 * refWidth &&
               thisHeight <= 16 * refHeight;
    }

    public void SetupScaleFactorsForFrame(int otherW, int otherH, int thisW, int thisH)
    {
        if (!ValidRefFrameSize(otherW, otherH, thisW, thisH))
        {
            XScaleFP = RefInvalidScale;
            YScaleFP = RefInvalidScale;

            return;
        }

        XScaleFP = GetFixedPointScaleFactor(otherW, thisW);
        YScaleFP = GetFixedPointScaleFactor(otherH, thisH);
        XStepQ4 = ScaledX(16);
        YStepQ4 = ScaledY(16);
    }
}