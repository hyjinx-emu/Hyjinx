using LibHac.Common;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace LibHac.Tests;

public class BufferStructTests
{
    [Test]
    public static void BufferIndexer()
    {
        var buffer = new Buffer16();

        buffer[0] = 5;
        buffer[1] = 6;

        Assert.That(buffer[0], Is.EqualTo(5));
        Assert.That(buffer[1], Is.EqualTo(6));
    }

    [Test]
    public static void CastBufferToByteSpan()
    {
        var buffer = new Buffer16();

        Span<byte> byteSpan = buffer.Bytes;

        Assert.That(byteSpan.Length, Is.EqualTo(16));
        Assert.That(Unsafe.AreSame(ref Unsafe.As<Buffer16, byte>(ref buffer), ref byteSpan[0]), Is.True);
    }

    [Test]
    public static void CastBufferToByteSpanImplicit()
    {
        var buffer = new Buffer16();

        Span<byte> byteSpan = buffer;

        Assert.That(byteSpan.Length, Is.EqualTo(16));
        Assert.That(Unsafe.AreSame(ref Unsafe.As<Buffer16, byte>(ref buffer), ref byteSpan[0]), Is.True);
    }

    [Test]
    public static void CastBufferToReadOnlyByteSpanImplicit()
    {
        var buffer = new Buffer16();

        ReadOnlySpan<byte> byteSpan = buffer;

        Assert.That(byteSpan.Length, Is.EqualTo(16));
        Assert.That(Unsafe.AreSame(ref Unsafe.As<Buffer16, byte>(ref buffer), ref Unsafe.AsRef(byteSpan[0])), Is.True);
    }

    [Test]
    public static void CastBufferToSpan()
    {
        var buffer = new Buffer16();

        Span<ulong> ulongSpan = buffer.AsSpan<ulong>();

        Assert.That(ulongSpan.Length, Is.EqualTo(2));
        Assert.That(Unsafe.AreSame(ref Unsafe.As<Buffer16, ulong>(ref buffer), ref ulongSpan[0]), Is.True);
    }

    [Test]
    public static void CastBufferToStruct()
    {
        var buffer = new Buffer16();

        ref ulong ulongSpan = ref buffer.As<ulong>();

        Assert.That(Unsafe.AreSame(ref Unsafe.As<Buffer16, ulong>(ref buffer), ref ulongSpan), Is.True);
    }

    [Test]
    public static void CastBufferToLargerStruct()
    {
        var buffer = new Buffer16();

        Assert.Throws<ArgumentException>(() => buffer.As<Struct32Bytes>());
    }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    private struct Struct32Bytes { }
}