using Hyjinx.HLE.HOS.Kernel.Common;
using Hyjinx.HLE.HOS.Kernel.Process;

namespace Hyjinx.HLE.HOS.Kernel.Ipc;

class KSession : KAutoObject
{
    public KServerSession ServerSession { get; }
    public KClientSession ClientSession { get; }

    private readonly bool _hasBeenInitialized;

    public KSession(KernelContext context, KClientPort parentPort = null) : base(context)
    {
        IncrementReferenceCount();

        ServerSession = new KServerSession(context, this);
        ClientSession = new KClientSession(context, this, parentPort);

        _hasBeenInitialized = true;
    }

    public void DisconnectClient()
    {
        if (ClientSession.State == ChannelState.Open)
        {
            ClientSession.State = ChannelState.ClientDisconnected;

            ServerSession.CancelAllRequestsClientDisconnected();
        }
    }

    public void DisconnectServer()
    {
        if (ClientSession.State == ChannelState.Open)
        {
            ClientSession.State = ChannelState.ServerDisconnected;
        }
    }

    protected override void Destroy()
    {
        if (_hasBeenInitialized)
        {
            ClientSession.DisconnectFromPort();

            KProcess creatorProcess = ClientSession.CreatorProcess;

            creatorProcess.ResourceLimit?.Release(LimitableResource.Session, 1);
            creatorProcess.DecrementReferenceCount();
        }
    }
}