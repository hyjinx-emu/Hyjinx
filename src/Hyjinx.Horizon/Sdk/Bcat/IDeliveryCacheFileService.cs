using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;
using LibHac.Bcat;
using System;

namespace Hyjinx.Horizon.Sdk.Bcat;

internal interface IDeliveryCacheFileService : IServiceObject
{
    Result GetDigest(out Digest digest);
    Result GetSize(out long size);
    Result Open(DirectoryName directoryName, FileName fileName);
    Result Read(long offset, out long bytesRead, Span<byte> data);
}