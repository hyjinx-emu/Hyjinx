using LibHac.Os.Impl;
using System;

namespace LibHac.Os;

public class OsState : IDisposable
{
    internal const int InitialProcessCountMin = 1;
    internal const int InitialProcessCountMax = 0x50;

    public OsStateImpl Impl => new OsStateImpl(this);
    internal HorizonClient Hos { get; }
    internal OsResourceManager ResourceManager { get; }

    // Todo: Use configuration object if/when more options are added
    internal OsState(HorizonClient horizonClient, ITickGenerator tickGenerator)
    {
        Hos = horizonClient;
        ResourceManager = new OsResourceManager(tickGenerator);
    }

    public ProcessId GetCurrentProcessId()
    {
        return Hos.ProcessId;
    }

    public void Dispose()
    {
        ResourceManager.Dispose();
    }
}

// Functions in the nn::os::detail namespace use this struct.
public readonly struct OsStateImpl
{
    internal readonly OsState Os;
    internal HorizonClient Hos => Os.Hos;

    internal OsStateImpl(OsState parent) => Os = parent;
}