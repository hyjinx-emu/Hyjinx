using LibHac.Common;
using LibHac.Svc;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LibHac.Sm;

// This is basically a makeshift service manager that doesn't do anything
// other than keep service objects for now. It's just here so other stuff
// isn't blocked waiting for something better.
internal class ServiceManager
{
    private Dictionary<ServiceName, IServiceObject> Services { get; } = new Dictionary<ServiceName, IServiceObject>();

    internal Result GetService(ref SharedRef<IDisposable> outServiceObject, ServiceName serviceName)
    {
        Result res = ValidateServiceName(serviceName);
        if (res.IsFailure())
            return res.Miss();

        if (!Services.TryGetValue(serviceName, out IServiceObject service))
        {
            return ResultSvc.NotFound.Log();
        }

        return service.GetServiceObject(ref outServiceObject);
    }

    internal Result RegisterService(IServiceObject service, ServiceName serviceName)
    {
        Result res = ValidateServiceName(serviceName);
        if (res.IsFailure())
            return res.Miss();

        if (!Services.TryAdd(serviceName, service))
        {
            return ResultSm.AlreadyRegistered.Log();
        }

        return Result.Success;
    }

    internal Result UnregisterService(ServiceName serviceName)
    {
        Result res = ValidateServiceName(serviceName);
        if (res.IsFailure())
            return res.Miss();

        if (!Services.Remove(serviceName, out IServiceObject service))
        {
            return ResultSm.NotRegistered.Log();
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (service is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return Result.Success;
    }

    private Result ValidateServiceName(ServiceName name)
    {
        // Service names must be non-empty.
        if (name.Name == 0)
            return ResultSm.InvalidServiceName.Log();

        // Get name length.
        int nameLen;
        for (nameLen = 1; nameLen < Unsafe.SizeOf<ServiceName>(); nameLen++)
        {
            if (SpanHelpers.AsReadOnlyByteSpan(in name)[nameLen] == 0)
            {
                break;
            }
        }

        // Names must be all-zero after they end.
        for (; nameLen < Unsafe.SizeOf<ServiceName>(); nameLen++)
        {
            if (SpanHelpers.AsReadOnlyByteSpan(in name)[nameLen] != 0)
            {
                return ResultSm.InvalidServiceName.Log();
            }
        }

        return Result.Success;
    }
}