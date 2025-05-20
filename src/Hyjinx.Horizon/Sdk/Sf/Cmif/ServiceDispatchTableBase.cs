using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf.Hipc;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hyjinx.Horizon.Sdk.Sf.Cmif;

abstract partial class ServiceDispatchTableBase
{
    private const uint MaxCmifVersion = 1;

    private static readonly ILogger _logger = Logger.DefaultLoggerFactory.CreateLogger<ServiceDispatchTableBase>();

    public abstract Result ProcessMessage(ref ServiceDispatchContext context, ReadOnlySpan<byte> inRawData);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "Request message size 0x{length:X} is invalid.")]
    private static partial void LogRequestSizeInvalid(ILogger logger, int length);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "Request message header magic value 0x{magic:X} is invalid.")]
    private static partial void LogRequestMagicInvalid(ILogger logger, uint magic);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "Missing service {objectName} (command ID: {commandId}) ignored")]
    private static partial void LogServiceNotFound(ILogger logger, string objectName, uint commandId);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "{commandHandler} returned error {result}")]
    private static partial void LogCommandHandlerError(ILogger logger, string commandHandler, Result result);

    protected static Result ProcessMessageImpl(ref ServiceDispatchContext context, ReadOnlySpan<byte> inRawData, IReadOnlyDictionary<int, CommandHandler> entries, string objectName)
    {
        if (inRawData.Length < Unsafe.SizeOf<CmifInHeader>())
        {
            LogRequestSizeInvalid(_logger, inRawData.Length);

            return SfResult.InvalidHeaderSize;
        }

        CmifInHeader inHeader = MemoryMarshal.Cast<byte, CmifInHeader>(inRawData)[0];

        if (inHeader.Magic != CmifMessage.CmifInHeaderMagic || inHeader.Version > MaxCmifVersion)
        {
            LogRequestMagicInvalid(_logger, inHeader.Magic);

            return SfResult.InvalidInHeader;
        }

        ReadOnlySpan<byte> inMessageRawData = inRawData[Unsafe.SizeOf<CmifInHeader>()..];
        uint commandId = inHeader.CommandId;

        var outHeader = Span<CmifOutHeader>.Empty;

        if (!entries.TryGetValue((int)commandId, out var commandHandler))
        {
            if (HorizonStatic.Options.IgnoreMissingServices)
            {
                // If ignore missing services is enabled, just pretend that everything is fine.
                PrepareForStubReply(ref context, out Span<byte> outRawData);
                CommandHandler.GetCmifOutHeaderPointer(ref outHeader, ref outRawData);
                outHeader[0] = new CmifOutHeader { Magic = CmifMessage.CmifOutHeaderMagic, Result = Result.Success };

                LogServiceNotFound(_logger, objectName, commandId);

                return Result.Success;
            }
            else if (HorizonStatic.Options.ThrowOnInvalidCommandIds)
            {
                throw new NotImplementedException($"{objectName} command ID: {commandId} is not implemented");
            }

            return SfResult.UnknownCommandId;
        }

        LogMethodCalled(_logger, objectName, commandHandler.MethodName);

        Result commandResult = commandHandler.Invoke(ref outHeader, ref context, inMessageRawData);

        if (commandResult.Module == SfResult.ModuleId ||
            commandResult.Module == HipcResult.ModuleId)
        {
            LogCommandHandlerError(_logger, commandHandler.MethodName, commandResult);
        }

        if (SfResult.RequestContextChanged(commandResult))
        {
            return commandResult;
        }

        if (outHeader.IsEmpty)
        {
            commandResult.AbortOnSuccess();

            return commandResult;
        }

        outHeader[0] = new CmifOutHeader { Magic = CmifMessage.CmifOutHeaderMagic, Result = commandResult };

        return Result.Success;
    }

    [LoggerMessage(LogLevel.Trace,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "{objectName}.{methodName} called")]
    private static partial void LogMethodCalled(ILogger logger, string objectName, string methodName);

    private static void PrepareForStubReply(scoped ref ServiceDispatchContext context, out Span<byte> outRawData)
    {
        var response = HipcMessage.WriteResponse(context.OutMessageBuffer, 0, 0x20 / sizeof(uint), 0, 0);
        outRawData = MemoryMarshal.Cast<uint, byte>(response.DataWords);
    }
}