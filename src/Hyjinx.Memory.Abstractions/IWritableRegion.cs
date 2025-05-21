using System;

namespace Hyjinx.Memory;

public interface IWritableRegion : IDisposable
{
    Memory<byte> Memory { get; }
}