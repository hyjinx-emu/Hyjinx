using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.Device;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Hyjinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Represents a Macro Just-in-Time compiler execution context.
    /// </summary>
    partial class MacroJitContext
    {
        private readonly ILogger<MacroJitContext> _logger = Logger.DefaultLoggerFactory.CreateLogger<MacroJitContext>();

        /// <summary>
        /// Arguments FIFO.
        /// </summary>
        public Queue<FifoWord> Fifo { get; } = new();

        /// <summary>
        /// Fetches a arguments from the arguments FIFO.
        /// </summary>
        /// <returns>The call argument, or 0 if the FIFO is empty</returns>
        public int FetchParam()
        {
            if (!Fifo.TryDequeue(out var value))
            {
                LogMacroAttemptedNonExistentArgument();

                return 0;
            }

            return value.Word;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
            Message = "Macro attempted to fetch an non-existent argument.")]
        private partial void LogMacroAttemptedNonExistentArgument();

        /// <summary>
        /// Reads data from a GPU register.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="reg">Register offset to read</param>
        /// <returns>GPU register value</returns>
        public static int Read(IDeviceState state, int reg)
        {
            return state.Read(reg * 4);
        }

        /// <summary>
        /// Performs a GPU method call.
        /// </summary>
        /// <param name="value">Call argument</param>
        /// <param name="state">Current GPU state</param>
        /// <param name="methAddr">Address, in words, of the method</param>
        public static void Send(int value, IDeviceState state, int methAddr)
        {
            state.Write(methAddr * 4, value);
        }
    }
}