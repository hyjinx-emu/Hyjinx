using System.Text.Json.Serialization;

namespace Hyjinx.HLE.HOS;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemoryManagerMode : byte
{
    SoftwarePageTable,
    HostMapped,
    HostMappedUnsafe,
}