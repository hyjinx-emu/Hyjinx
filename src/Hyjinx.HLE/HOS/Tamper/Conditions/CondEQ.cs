using Hyjinx.HLE.HOS.Tamper.Operations;

namespace Hyjinx.HLE.HOS.Tamper.Conditions;

class CondEQ<T> : ICondition where T : unmanaged
{
    private readonly IOperand _lhs;
    private readonly IOperand _rhs;

    public CondEQ(IOperand lhs, IOperand rhs)
    {
        _lhs = lhs;
        _rhs = rhs;
    }

    public bool Evaluate()
    {
        return (dynamic)_lhs.Get<T>() == (dynamic)_rhs.Get<T>();
    }
}