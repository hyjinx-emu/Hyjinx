using Hyjinx.Horizon.Common;
using System.Diagnostics;
using System.Threading;

namespace Hyjinx.HLE.HOS.Kernel.Common;

class KAutoObject
{
    protected KernelContext KernelContext;

    private int _referenceCount;

    public KAutoObject(KernelContext context)
    {
        KernelContext = context;

        _referenceCount = 1;
    }

    public virtual Result SetName(string name)
    {
        if (!KernelContext.AutoObjectNames.TryAdd(name, this))
        {
            return KernelResult.InvalidState;
        }

        return Result.Success;
    }

    public static Result RemoveName(KernelContext context, string name)
    {
        if (!context.AutoObjectNames.TryRemove(name, out _))
        {
            return KernelResult.NotFound;
        }

        return Result.Success;
    }

    public static KAutoObject FindNamedObject(KernelContext context, string name)
    {
        if (context.AutoObjectNames.TryGetValue(name, out KAutoObject obj))
        {
            return obj;
        }

        return null;
    }

    public void IncrementReferenceCount()
    {
        int newRefCount = Interlocked.Increment(ref _referenceCount);

        Debug.Assert(newRefCount >= 2);
    }

    public void DecrementReferenceCount()
    {
        int newRefCount = Interlocked.Decrement(ref _referenceCount);

        Debug.Assert(newRefCount >= 0);

        if (newRefCount == 0)
        {
            Destroy();
        }
    }

    protected virtual void Destroy()
    {
    }
}