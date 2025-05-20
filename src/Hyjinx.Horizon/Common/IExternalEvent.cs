using System;

namespace Hyjinx.Horizon.Common
{
    public interface IExternalEvent
    {
        void Signal();
        void Clear();
    }
}