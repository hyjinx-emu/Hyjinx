namespace Hyjinx.HLE.HOS.Tamper.Operations;

class OpLsh<T> : IOperation where T : unmanaged
{
    readonly IOperand _destination;
    readonly IOperand _lhs;
    readonly IOperand _rhs;

    public OpLsh(IOperand destination, IOperand lhs, IOperand rhs)
    {
        _destination = destination;
        _lhs = lhs;
        _rhs = rhs;
    }

    public void Execute()
    {
        _destination.Set((T)((dynamic)_lhs.Get<T>() << (dynamic)_rhs.Get<T>()));
    }
}