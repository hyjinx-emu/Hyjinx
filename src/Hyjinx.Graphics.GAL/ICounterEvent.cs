using System;

namespace Hyjinx.Graphics.GAL
{
    public interface ICounterEvent : IDisposable
    {
        bool Invalid { get; set; }

        bool ReserveForHostAccess();

        void Flush();
    }
}
