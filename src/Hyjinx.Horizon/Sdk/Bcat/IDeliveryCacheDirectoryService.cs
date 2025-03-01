using LibHac.Bcat;
using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;
using System;

namespace Hyjinx.Horizon.Sdk.Bcat
{
    internal interface IDeliveryCacheDirectoryService : IServiceObject
    {
        Result GetCount(out int count);
        Result Open(DirectoryName directoryName);
        Result Read(out int entriesRead, Span<DeliveryCacheDirectoryEntry> entriesBuffer);
    }
}
