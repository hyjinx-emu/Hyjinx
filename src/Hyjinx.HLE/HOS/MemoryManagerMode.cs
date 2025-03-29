namespace Hyjinx.HLE.HOS;

public enum MemoryManagerMode : byte
{
    SoftwarePageTable,
    HostMapped,
    HostMappedUnsafe,
}
