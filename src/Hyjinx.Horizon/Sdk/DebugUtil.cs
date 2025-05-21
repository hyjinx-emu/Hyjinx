using System.Diagnostics;

namespace Hyjinx.Horizon.Sdk;

static class DebugUtil
{
    public static void Assert(bool condition)
    {
        Debug.Assert(condition);
    }
}