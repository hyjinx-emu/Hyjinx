using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Bcat;
using Hyjinx.Horizon.Sdk.Sf;
using Hyjinx.Horizon.Sdk.Sf.Hipc;
using LibHac.Bcat;
using LibHac.Common;
using System;
using System.Threading;

namespace Hyjinx.Horizon.Bcat.Ipc;

partial class DeliveryCacheStorageService : IDeliveryCacheStorageService, IDisposable
{
    private SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService> _libHacService;
    private int _disposalState;

    public DeliveryCacheStorageService(ref SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService> libHacService)
    {
        _libHacService = SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheStorageService>.CreateMove(ref libHacService);
    }

    [CmifCommand(0)]
    public Result CreateFileService(out IDeliveryCacheFileService service)
    {
        using var libHacService = new SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheFileService>();

        var resultCode = _libHacService.Get.CreateFileService(ref libHacService.Ref);

        if (resultCode.IsSuccess())
        {
            service = new DeliveryCacheFileService(ref libHacService.Ref);
        }
        else
        {
            service = null;
        }

        return resultCode.ToHorizonResult();
    }

    [CmifCommand(1)]
    public Result CreateDirectoryService(out IDeliveryCacheDirectoryService service)
    {
        using var libHacService = new SharedRef<LibHac.Bcat.Impl.Ipc.IDeliveryCacheDirectoryService>();

        var resultCode = _libHacService.Get.CreateDirectoryService(ref libHacService.Ref);

        if (resultCode.IsSuccess())
        {
            service = new DeliveryCacheDirectoryService(ref libHacService.Ref);
        }
        else
        {
            service = null;
        }

        return resultCode.ToHorizonResult();
    }

    [CmifCommand(10)]
    public Result EnumerateDeliveryCacheDirectory(out int count, [Buffer(HipcBufferFlags.Out | HipcBufferFlags.MapAlias)] Span<DirectoryName> directoryNames)
    {
        return _libHacService.Get.EnumerateDeliveryCacheDirectory(out count, directoryNames).ToHorizonResult();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposalState, 1) == 0)
        {
            _libHacService.Destroy();
        }
    }
}