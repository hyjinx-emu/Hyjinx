using Hyjinx.HLE.HOS.Tamper.Operations;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Hyjinx.HLE.HOS.Tamper;

class Pointer : IOperand
{
    private static readonly ILogger<Pointer> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<Pointer>();

    private readonly IOperand _position;
    private readonly ITamperedProcess _process;

    public Pointer(IOperand position, ITamperedProcess process)
    {
        _position = position;
        _process = process;
    }

    public T Get<T>() where T : unmanaged
    {
        return _process.ReadMemory<T>(_position.Get<ulong>());
    }

    public void Set<T>(T value) where T : unmanaged
    {
        ulong position = _position.Get<ulong>();

        _logger.LogDebug(new EventId((int)LogClass.TamperMachine, nameof(LogClass.TamperMachine)),
            "0x{position:X16}@{size:X}: {value:X}", position, Unsafe.SizeOf<T>(), value);

        _process.WriteMemory(position, value);
    }
}