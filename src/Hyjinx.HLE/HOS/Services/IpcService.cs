using Hyjinx.HLE.Exceptions;
using Hyjinx.HLE.HOS.Ipc;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Hyjinx.HLE.HOS.Services;

/// <summary>
/// A service which handles inter-process communication capabilities.
/// </summary>
internal interface IpcService
{
    IReadOnlyDictionary<int, MethodInfo> CmifCommands { get; }

    IReadOnlyDictionary<int, MethodInfo> TipcCommands { get; }

    ServerBase Server { get; }

    IpcService Parent { get; set; }

    bool IsDomain { get; }

    int ConvertToDomain();

    void ConvertToSession();

    void CallCmifMethod(ServiceCtx context);

    void CallTipcMethod(ServiceCtx context);

    bool TrySetServer(ServerBase newServer);

    void SetParent(IpcService parent);

    void DestroyAtExit();

    int Add(IpcService obj);

    IpcService GetObject(int id);
}

/// <summary>
/// An abstract <see cref="IpcService"/> which contains base capabilities for all IPC services.
/// </summary>
/// <typeparam name="T">The type of <see cref="IpcService{T}"/> being implemented.</typeparam>
internal abstract partial class IpcService<T> : IpcService
    where T : IpcService<T>
{
    protected static readonly ILogger<T> _logger =
        Logger.DefaultLoggerFactory.CreateLogger<T>();

    public IReadOnlyDictionary<int, MethodInfo> CmifCommands { get; }
    public IReadOnlyDictionary<int, MethodInfo> TipcCommands { get; }

    public ServerBase Server { get; private set; }

    public IpcService Parent { get; set; }
    private readonly IdDictionary _domainObjects;
    private int _selfId;
    public bool IsDomain { get; private set; }

    protected IpcService(ServerBase? server = null)
    {
        CmifCommands = typeof(IpcService).Assembly.GetTypes()
            .Where(type => type == GetType())
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            .SelectMany(methodInfo => methodInfo.GetCustomAttributes(typeof(CommandCmifAttribute))
            .Select(command => (((CommandCmifAttribute)command).Id, methodInfo)))
            .ToDictionary(command => command.Id, command => command.methodInfo);

        TipcCommands = typeof(IpcService).Assembly.GetTypes()
            .Where(type => type == GetType())
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            .SelectMany(methodInfo => methodInfo.GetCustomAttributes(typeof(CommandTipcAttribute))
            .Select(command => (((CommandTipcAttribute)command).Id, methodInfo)))
            .ToDictionary(command => command.Id, command => command.methodInfo);

        Server = server!;

        Parent = this;
        _domainObjects = new IdDictionary();
        _selfId = -1;
    }

    public int ConvertToDomain()
    {
        if (_selfId == -1)
        {
            _selfId = _domainObjects.Add(this);
        }

        IsDomain = true;

        return _selfId;
    }

    public void ConvertToSession()
    {
        IsDomain = false;
    }

    public void CallCmifMethod(ServiceCtx context)
    {
        IpcService service = this;

        if (IsDomain)
        {
            int domainWord0 = context.RequestData.ReadInt32();
            int domainObjId = context.RequestData.ReadInt32();

            int domainCmd = (domainWord0 >> 0) & 0xff;
            int inputObjCount = (domainWord0 >> 8) & 0xff;
            int dataPayloadSize = (domainWord0 >> 16) & 0xffff;

            context.RequestData.BaseStream.Seek(0x10 + dataPayloadSize, SeekOrigin.Begin);

            context.Request.ObjectIds.EnsureCapacity(inputObjCount);

            for (int index = 0; index < inputObjCount; index++)
            {
                context.Request.ObjectIds.Add(context.RequestData.ReadInt32());
            }

            context.RequestData.BaseStream.Seek(0x10, SeekOrigin.Begin);

            if (domainCmd == 1)
            {
                service = GetObject(domainObjId);

                context.ResponseData.Write(0L);
                context.ResponseData.Write(0L);
            }
            else if (domainCmd == 2)
            {
                Delete(domainObjId);

                context.ResponseData.Write(0L);

                return;
            }
            else
            {
                throw new NotImplementedException($"Domain command: {domainCmd}");
            }
        }

#pragma warning disable IDE0059 // Remove unnecessary value assignment
        long sfciMagic = context.RequestData.ReadInt64();
#pragma warning restore IDE0059
        int commandId = (int)context.RequestData.ReadInt64();

        bool serviceExists = service.CmifCommands.TryGetValue(commandId, out MethodInfo processRequest);

        if (context.Device.Configuration.IgnoreMissingServices || serviceExists)
        {
            ResultCode result = ResultCode.Success;

            context.ResponseData.BaseStream.Seek(IsDomain ? 0x20 : 0x10, SeekOrigin.Begin);

            if (serviceExists)
            {
                LogRequestReceived(service.GetType().Name, processRequest!.Name);

                result = (ResultCode)processRequest.Invoke(service, new object[] { context });
            }
            else
            {
                string serviceName;


                serviceName = (service is not DummyService dummyService) ? service.GetType().FullName : dummyService.ServiceName;

                LogMissingService(serviceName!, commandId);
            }

            if (IsDomain)
            {
                foreach (int id in context.Response.ObjectIds)
                {
                    context.ResponseData.Write(id);
                }

                context.ResponseData.BaseStream.Seek(0, SeekOrigin.Begin);

                context.ResponseData.Write(context.Response.ObjectIds.Count);
            }

            context.ResponseData.BaseStream.Seek(IsDomain ? 0x10 : 0, SeekOrigin.Begin);

            context.ResponseData.Write(IpcMagic.Sfco);
            context.ResponseData.Write((long)result);
        }
        else
        {
            string dbgMessage = $"{service.GetType().FullName}: {commandId}";

            throw new ServiceNotImplementedException(service, context, dbgMessage);
        }
    }

    public void CallTipcMethod(ServiceCtx context)
    {
        int commandId = (int)context.Request.Type - 0x10;

        bool serviceExists = TipcCommands.TryGetValue(commandId, out MethodInfo processRequest);

        if (context.Device.Configuration.IgnoreMissingServices || serviceExists)
        {
            ResultCode result = ResultCode.Success;

            context.ResponseData.BaseStream.Seek(0x4, SeekOrigin.Begin);

            if (serviceExists)
            {
                LogRequestReceived(GetType().Name, processRequest!.Name);

                result = (ResultCode)processRequest.Invoke(this, new object[] { context });
            }
            else
            {
                string serviceName;


                serviceName = (this is not DummyService dummyService) ? GetType().FullName : dummyService.ServiceName;

                LogMissingService(serviceName!, commandId);
            }

            context.ResponseData.BaseStream.Seek(0, SeekOrigin.Begin);

            context.ResponseData.Write((uint)result);
        }
        else
        {
            string dbgMessage = $"{GetType().FullName}: {commandId}";

            throw new ServiceNotImplementedException(this, context, dbgMessage);
        }
    }

    [LoggerMessage(LogLevel.Trace,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "{serviceName}: {requestName}")]
    protected partial void LogRequestReceived(string serviceName, string requestName);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.KernelIpc, EventName = nameof(LogClass.KernelIpc),
        Message = "Missing service {serviceName}: {commandId} ignored.")]
    protected partial void LogMissingService(string serviceName, int commandId);

    protected void MakeObject(ServiceCtx context, IpcService obj)
    {
        obj.TrySetServer(Parent.Server);

        if (Parent.IsDomain)
        {
            obj.Parent = Parent;

            context.Response.ObjectIds.Add(Parent.Add(obj));
        }
        else
        {
            context.Device.System.KernelContext.Syscall.CreateSession(out int serverSessionHandle, out int clientSessionHandle, false, 0);

            obj.Server.AddSessionObj(serverSessionHandle, obj);

            context.Response.HandleDesc = IpcHandleDesc.MakeMove(clientSessionHandle);
        }
    }

    protected T GetObject<T>(ServiceCtx context, int index) where T : class, IpcService
    {
        int objId = context.Request.ObjectIds[index];

        IpcService obj = Parent.GetObject(objId);

        return obj is T t ? t : null;
    }

    public bool TrySetServer(ServerBase newServer)
    {
        if (Server == null)
        {
            Server = newServer;

            return true;
        }

        return false;
    }

    public int Add(IpcService obj)
    {
        return _domainObjects.Add(obj);
    }

    private bool Delete(int id)
    {
        object obj = _domainObjects.Delete(id);

        if (obj is IDisposable disposableObj)
        {
            disposableObj.Dispose();
        }

        return obj != null;
    }

    public IpcService GetObject(int id)
    {
        return _domainObjects.GetData<IpcService>(id);
    }

    public void SetParent(IpcService parent)
    {
        Parent = parent.Parent;
    }

    public virtual void DestroyAtExit()
    {
        foreach (object domainObject in _domainObjects.Values)
        {
            if (domainObject != this && domainObject is IDisposable disposableObj)
            {
                disposableObj.Dispose();
            }
        }

        _domainObjects.Clear();
    }
}