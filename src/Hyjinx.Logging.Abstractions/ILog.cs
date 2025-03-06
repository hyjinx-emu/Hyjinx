using System.Runtime.CompilerServices;

namespace Hyjinx.Common.Logging;

public interface ILog
{
    void PrintMsg(LogClass logClass, string message);

    void Print(LogClass logClass, string message, [CallerMemberName] string caller = "");

    void Print(LogClass logClass, string message, object data, [CallerMemberName] string caller = "");

    void PrintStack(LogClass logClass, string message, [CallerMemberName] string caller = "");

    void PrintStub(LogClass logClass, string message = "", [CallerMemberName] string caller = "");

    void PrintStub(LogClass logClass, object data, [CallerMemberName] string caller = "");

    void PrintStub(LogClass logClass, string message, object data, [CallerMemberName] string caller = "");

    void PrintRawMsg(string message);
}
