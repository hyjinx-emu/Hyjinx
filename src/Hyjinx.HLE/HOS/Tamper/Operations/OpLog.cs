using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Hyjinx.HLE.HOS.Tamper.Operations;

class OpLog<T> : IOperation where T : unmanaged
{
    private static readonly ILogger<OpLog<T>> _logger = Logger.DefaultLoggerFactory.CreateLogger<OpLog<T>>();

    readonly int _logId;
    readonly IOperand _source;

    public OpLog(int logId, IOperand source)
    {
        _logId = logId;
        _source = source;
    }

    public void Execute()
    {
        _logger.LogDebug(new EventId((int)LogClass.TamperMachine, nameof(LogClass.TamperMachine)),
            "Tamper debug log id={id} value={value:X}", _logId, _source.Get<T>());
    }
}