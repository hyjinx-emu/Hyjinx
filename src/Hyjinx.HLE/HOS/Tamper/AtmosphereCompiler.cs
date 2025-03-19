using Hyjinx.Logging.Abstractions;
using Hyjinx.HLE.Exceptions;
using Hyjinx.HLE.HOS.Tamper.CodeEmitters;
using Hyjinx.HLE.HOS.Tamper.Operations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Hyjinx.HLE.HOS.Tamper
{
    partial class AtmosphereCompiler
    {
        private static readonly ILogger<AtmosphereCompiler> _logger = Logger.DefaultLoggerFactory.CreateLogger<AtmosphereCompiler>();
        private readonly ulong _exeAddress;
        private readonly ulong _heapAddress;
        private readonly ulong _aliasAddress;
        private readonly ulong _aslrAddress;
        private readonly ITamperedProcess _process;

        public AtmosphereCompiler(ulong exeAddress, ulong heapAddress, ulong aliasAddress, ulong aslrAddress, ITamperedProcess process)
        {
            _exeAddress = exeAddress;
            _heapAddress = heapAddress;
            _aliasAddress = aliasAddress;
            _aslrAddress = aslrAddress;
            _process = process;
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "Compiling Atmosphere cheat {name}...\n{addresses}")]
        private partial void LogCompilingCheat(string name, string addresses);

        public ITamperProgram Compile(string name, IEnumerable<string> rawInstructions)
        {
            string[] addresses = {
                $"    Executable address: 0x{_exeAddress:X16}",
                $"    Heap address      : 0x{_heapAddress:X16}",
                $"    Alias address     : 0x{_aliasAddress:X16}",
                $"    Aslr address      : 0x{_aslrAddress:X16}",
            };

            LogCompilingCheat(name, string.Join('\n', addresses));

            try
            {
                return CompileImpl(name, rawInstructions);
            }
            catch (TamperCompilationException ex)
            {
                // Just print the message without the stack trace.
                _logger.LogError(new EventId((int)LogClass.TamperMachine, nameof(LogClass.TamperMachine)), ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId((int)LogClass.TamperMachine, nameof(LogClass.TamperMachine)), ex, ex.Message);
            }

            LogErrorWhileCompilingAtmosphereCheat();

            return null;
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "There was a problem while compiling the Atmosphere cheat")]
        private partial void LogErrorWhileCompilingAtmosphereCheat();

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.TamperMachine, EventName = nameof(LogClass.TamperMachine),
            Message = "Compiling instruction {rawInstruction}")]
        private partial void LogCompilingInstruction(string rawInstruction);
        
        private ITamperProgram CompileImpl(string name, IEnumerable<string> rawInstructions)
        {
            CompilationContext context = new(_exeAddress, _heapAddress, _aliasAddress, _aslrAddress, _process);
            context.BlockStack.Push(new OperationBlock(null));

            // Parse the instructions.

            foreach (string rawInstruction in rawInstructions)
            {
                LogCompilingInstruction(rawInstruction);

                byte[] instruction = InstructionHelper.ParseRawInstruction(rawInstruction);
                CodeType codeType = InstructionHelper.GetCodeType(instruction);

                switch (codeType)
                {
                    case CodeType.StoreConstantToAddress:
                        StoreConstantToAddress.Emit(instruction, context);
                        break;
                    case CodeType.BeginMemoryConditionalBlock:
                        BeginConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.EndConditionalBlock:
                        EndConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.StartEndLoop:
                        StartEndLoop.Emit(instruction, context);
                        break;
                    case CodeType.LoadRegisterWithContant:
                        LoadRegisterWithConstant.Emit(instruction, context);
                        break;
                    case CodeType.LoadRegisterWithMemory:
                        LoadRegisterWithMemory.Emit(instruction, context);
                        break;
                    case CodeType.StoreConstantToMemory:
                        StoreConstantToMemory.Emit(instruction, context);
                        break;
                    case CodeType.LegacyArithmetic:
                        LegacyArithmetic.Emit(instruction, context);
                        break;
                    case CodeType.BeginKeypressConditionalBlock:
                        BeginConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.Arithmetic:
                        Arithmetic.Emit(instruction, context);
                        break;
                    case CodeType.StoreRegisterToMemory:
                        StoreRegisterToMemory.Emit(instruction, context);
                        break;
                    case CodeType.BeginRegisterConditionalBlock:
                        BeginConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.SaveOrRestoreRegister:
                        SaveOrRestoreRegister.Emit(instruction, context);
                        break;
                    case CodeType.SaveOrRestoreRegisterWithMask:
                        SaveOrRestoreRegisterWithMask.Emit(instruction, context);
                        break;
                    case CodeType.ReadOrWriteStaticRegister:
                        ReadOrWriteStaticRegister.Emit(instruction, context);
                        break;
                    case CodeType.PauseProcess:
                        PauseProcess.Emit(instruction, context);
                        break;
                    case CodeType.ResumeProcess:
                        ResumeProcess.Emit(instruction, context);
                        break;
                    case CodeType.DebugLog:
                        DebugLog.Emit(instruction, context);
                        break;
                    default:
                        throw new TamperCompilationException($"Code type {codeType} not implemented in Atmosphere cheat");
                }
            }

            // Initialize only the registers used.

            Value<ulong> zero = new(0UL);
            int position = 0;

            foreach (Register register in context.Registers.Values)
            {
                context.CurrentOperations.Insert(position, new OpMov<ulong>(register, zero));
                position++;
            }

            if (context.BlockStack.Count != 1)
            {
                throw new TamperCompilationException("Reached end of compilation with unmatched conditional(s) or loop(s)");
            }

            return new AtmosphereProgram(name, _process, context.PressedKeys, new Block(context.CurrentOperations));
        }
    }
}
