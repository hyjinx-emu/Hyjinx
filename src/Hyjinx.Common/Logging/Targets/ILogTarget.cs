using System;

namespace Hyjinx.Common.Logging.Targets
{
    public interface ILogTarget : IDisposable
    {
        void Log(object sender, LogEventArgs args);

        string Name { get; }
    }
}
