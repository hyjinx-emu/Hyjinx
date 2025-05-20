using Hyjinx.HLE.HOS.Tamper.Operations;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Hyjinx.HLE.HOS.Tamper;

partial class Register : IOperand
{
    private static readonly ILogger<Register> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<Register>();

    private ulong _register = 0;
    private readonly string _alias;

    public Register(string alias)
    {
        _alias = alias;
    }

    public T Get<T>() where T : unmanaged
    {
        return (T)(dynamic)_register;
    }

    public void Set<T>(T value) where T : unmanaged
    {
        LogValueSet(_alias, value);

        _register = (ulong)(dynamic)value;
    }

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
        Message = "{alias}: {value}")]
    private partial void LogValueSet(string alias, object value);
}