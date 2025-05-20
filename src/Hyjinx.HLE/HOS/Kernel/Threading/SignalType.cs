namespace Hyjinx.HLE.HOS.Kernel.Threading
{
    enum SignalType
    {
        Signal = 0,
        SignalAndIncrementIfEqual = 1,
        SignalAndModifyIfEqual = 2,
    }
}