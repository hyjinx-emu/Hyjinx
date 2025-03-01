using Hyjinx.HLE.HOS.Kernel.Common;

namespace Hyjinx.HLE.HOS.Kernel.Ipc
{
    class KLightSession : KAutoObject
    {
        public KLightServerSession ServerSession { get; }
        public KLightClientSession ClientSession { get; }

        public KLightSession(KernelContext context) : base(context)
        {
            ServerSession = new KLightServerSession(context, this);
            ClientSession = new KLightClientSession(context, this);
        }
    }
}
