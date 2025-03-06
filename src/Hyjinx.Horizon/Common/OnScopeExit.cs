using System;

namespace Hyjinx.Horizon.Common
{
    public readonly struct OnScopeExit : IDisposable
    {
        private readonly Action _action;

        public OnScopeExit(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}
