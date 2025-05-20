using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Hyjinx.Horizon.Sdk.Sf.Cmif;

abstract class ServerMessageProcessor
{
    public abstract void SetImplementationProcessor(ServerMessageProcessor impl);
    public abstract ServerMessageRuntimeMetadata GetRuntimeMetadata();

    public abstract Result PrepareForProcess(scoped ref ServiceDispatchContext context, ServerMessageRuntimeMetadata runtimeMetadata);
    public abstract Result GetInObjects(Span<ServiceObjectHolder> inObjects);
    public abstract HipcMessageData PrepareForReply(scoped ref ServiceDispatchContext context, out Span<byte> outRawData, ServerMessageRuntimeMetadata runtimeMetadata);
    public abstract void PrepareForErrorReply(scoped ref ServiceDispatchContext context, out Span<byte> outRawData, ServerMessageRuntimeMetadata runtimeMetadata);
    public abstract void SetOutObjects(scoped ref ServiceDispatchContext context, HipcMessageData response, Span<ServiceObjectHolder> outObjects);
}