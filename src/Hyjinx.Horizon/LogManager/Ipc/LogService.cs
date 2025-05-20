using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Lm;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.LogManager.Ipc
{
    partial class LogService : ILogService
    {
        public LogDestination LogDestination { get; set; } = LogDestination.TargetManager;

        [CmifCommand(0)]
        public Result OpenLogger(out LmLogger logger, [ClientProcessId] ulong pid)
        {
            // NOTE: Internal name is Logger, but we rename it to LmLogger to avoid name clash with Hyjinx.Logging.Abstractions logger.
            logger = new LmLogger(this, pid);

            return Result.Success;
        }
    }
}