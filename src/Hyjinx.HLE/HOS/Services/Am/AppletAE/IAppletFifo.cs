using System;
using System.Collections.Concurrent;

namespace Hyjinx.HLE.HOS.Services.Am.AppletAE;

interface IAppletFifo<T> : IProducerConsumerCollection<T>
{
    event EventHandler DataAvailable;
}