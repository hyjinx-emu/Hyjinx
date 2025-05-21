using LibHac.Sf;
using System;

namespace LibHac.FsSrv.Sf;

public interface IEventNotifier : IDisposable
{
    Result GetEventHandle(out NativeHandle handle);
}