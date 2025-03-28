using Hyjinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Hyjinx.HLE.HOS
{
    [JsonConverter(typeof(TypedStringEnumConverter<MemoryManagerMode>))]
    public enum MemoryManagerMode : byte
    {
        SoftwarePageTable,
        HostMapped,
        HostMappedUnsafe,
    }
}
