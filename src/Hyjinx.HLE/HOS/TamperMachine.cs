using Hyjinx.Common.Logging;
using Hyjinx.HLE.HOS.Kernel;
using Hyjinx.HLE.HOS.Kernel.Process;
using Hyjinx.HLE.HOS.Services.Hid;
using Hyjinx.HLE.HOS.Tamper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Hyjinx.HLE.HOS
{
    public partial class TamperMachine
    {
        // Atmosphere specifies a delay of 83 milliseconds between the execution of the last
        // cheat and the re-execution of the first one.
        private const int TamperMachineSleepMs = 1000 / 12;

        private static readonly ILogger<TamperMachine> _logger = Logger.DefaultLoggerFactory.CreateLogger<TamperMachine>();
        
        private Thread _tamperThread = null;
        private readonly ConcurrentQueue<ITamperProgram> _programs = new();
        private long _pressedKeys = 0;
        private readonly Dictionary<string, ITamperProgram> _programDictionary = new();
        
        private void Activate()
        {
            if (_tamperThread == null || !_tamperThread.IsAlive)
            {
                _tamperThread = new Thread(this.TamperRunner)
                {
                    Name = "HLE.TamperMachine",
                };
                _tamperThread.Start();
            }
        }

        internal void InstallAtmosphereCheat(string name, string buildId, IEnumerable<string> rawInstructions, ProcessTamperInfo info, ulong exeAddress)
        {
            if (!CanInstallOnPid(info.Process.Pid))
            {
                return;
            }

            ITamperedProcess tamperedProcess = new TamperedKProcess(info.Process);
            AtmosphereCompiler compiler = new(exeAddress, info.HeapAddress, info.AliasAddress, info.AslrAddress, tamperedProcess);
            ITamperProgram program = compiler.Compile(name, rawInstructions);

            if (program != null)
            {
                program.TampersCodeMemory = false;

                _programs.Enqueue(program);
                _programDictionary.TryAdd($"{buildId}-{name}", program);
            }

            Activate();
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "Refusing to tamper kernel process {pid}.")]
        private static partial void LogRefusingToTamperKernel(ILogger logger, ulong pid);

        private static bool CanInstallOnPid(ulong pid)
        {
            // Do not allow tampering of kernel processes.
            if (pid < KernelConstants.InitialProcessId)
            {
                LogRefusingToTamperKernel(_logger, pid);

                return false;
            }

            return true;
        }

        public void EnableCheats(string[] enabledCheats)
        {
            foreach (var program in _programDictionary.Values)
            {
                program.IsEnabled = false;
            }

            foreach (var cheat in enabledCheats)
            {
                if (_programDictionary.TryGetValue(cheat, out var program))
                {
                    program.IsEnabled = true;
                }
            }
        }

        private static bool IsProcessValid(ITamperedProcess process)
        {
            return process.State != ProcessState.Crashed && process.State != ProcessState.Exiting && process.State != ProcessState.Exited;
        }

        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "TamperMachine thread running")]
        private partial void LogThreadStarted();
        
        private void TamperRunner()
        {
            LogThreadStarted();

            int sleepCounter = 0;

            while (true)
            {
                // Sleep to not consume too much CPU.
                if (sleepCounter == 0)
                {
                    sleepCounter = _programs.Count;
                    Thread.Sleep(TamperMachineSleepMs);
                }
                else
                {
                    sleepCounter--;
                }

                if (!AdvanceTamperingsQueue())
                {
                    // No more work to be done.
                    LogThreadExiting();

                    return;
                }
            }
        }
        
        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "TamperMachine thread exiting")]
        private partial void LogThreadExiting();

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "Running tampering program {name}")]
        private partial void LogRunningTamperingProgram(string name);

        private bool AdvanceTamperingsQueue()
        {
            if (!_programs.TryDequeue(out ITamperProgram program))
            {
                // No more programs in the queue.
                _programDictionary.Clear();

                return false;
            }

            // Check if the process is still suitable for running the tamper program.
            if (!IsProcessValid(program.Process))
            {
                // Exit without re-enqueuing the program because the process is no longer valid.
                return true;
            }

            // Re-enqueue the tampering program because the process is still valid.
            _programs.Enqueue(program);

            LogRunningTamperingProgram(program.Name);

            try
            {
                ControllerKeys pressedKeys = (ControllerKeys)Volatile.Read(ref _pressedKeys);
                program.Process.TamperedCodeMemory = false;
                program.Execute(pressedKeys);

                // Detect the first attempt to tamper memory and log it.
                if (!program.TampersCodeMemory && program.Process.TamperedCodeMemory)
                {
                    program.TampersCodeMemory = true;

                    LogTamperingProgramModifiesMemory(program.Name);
                }
            }
            catch (Exception ex)
            {
                LogTamperingProgramCrashedDuringStartup(program.Name, ex);
            }

            return true;
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "Tampering program {name} modifies code memory so it may not work properly.")]
        private partial void LogTamperingProgramModifiesMemory(string name);

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "The tampering program {name} crashed, this can happen while the game is starting.")]
        private partial void LogTamperingProgramCrashedDuringStartup(string name, Exception exception);

        public void UpdateInput(List<GamepadInput> gamepadInputs)
        {
            // Look for the input of the player one or the handheld.
            foreach (GamepadInput input in gamepadInputs)
            {
                if (input.PlayerId == PlayerIndex.Player1 || input.PlayerId == PlayerIndex.Handheld)
                {
                    Volatile.Write(ref _pressedKeys, (long)input.Buttons);

                    return;
                }
            }

            // Clear the input because player one is not conected.
            Volatile.Write(ref _pressedKeys, 0);
        }
    }
}
