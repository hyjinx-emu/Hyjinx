using Hyjinx.Logging.Abstractions;
using Hyjinx.HLE.HOS.Kernel.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.HOS.Services.Account.Acc.AsyncContext
{
    internal partial class AsyncExecution
    {
        private static readonly ILogger<AsyncExecution> _logger =
            Logger.DefaultLoggerFactory.CreateLogger<AsyncExecution>();

        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

        public KEvent SystemEvent { get; }
        public bool IsInitialized { get; private set; }
        public bool IsRunning { get; private set; }

        public AsyncExecution(KEvent asyncEvent)
        {
            SystemEvent = asyncEvent;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        public void Initialize(int timeout, Func<CancellationToken, Task> taskAsync)
        {
            Task.Run(async () =>
            {
                IsRunning = true;

                _tokenSource.CancelAfter(timeout);

                try
                {
                    await taskAsync(_token);
                }
                catch (Exception ex)
                {
                    LogUnexpectedErrorOccurred(ex);
                }

                SystemEvent.ReadableEvent.Signal();

                IsRunning = false;
            }, _token);

            IsInitialized = true;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceAcc, EventName = nameof(LogClass.ServiceAcc),
            Message = "An unexpected error occurred.")]
        private partial void LogUnexpectedErrorOccurred(Exception exception);

        public void Cancel()
        {
            _tokenSource.Cancel();
        }
    }
}
