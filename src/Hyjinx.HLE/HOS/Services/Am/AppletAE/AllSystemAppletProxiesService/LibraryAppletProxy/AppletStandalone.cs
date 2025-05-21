using System.Collections.Generic;

namespace Hyjinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy;

class AppletStandalone
{
    public AppletId AppletId;
    public LibraryAppletMode LibraryAppletMode;
    public Queue<byte[]> InputData;

    public AppletStandalone()
    {
        InputData = new Queue<byte[]>();
    }
}