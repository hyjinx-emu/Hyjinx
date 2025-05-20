using System;

namespace Hyjinx.Audio.Backends.Common;

public interface IDynamicRingBuffer
{
    int Length { get; }
    void Clear();
    void Clear(int size);
    void Write(ReadOnlySpan<byte> buffer, int index, int count);
    int Read(Span<byte> buffer, int index, int count);
}