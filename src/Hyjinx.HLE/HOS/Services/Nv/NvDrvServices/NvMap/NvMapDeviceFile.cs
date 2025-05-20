using Hyjinx.Common;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Graphics.Gpu.Memory;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;

namespace Hyjinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap
{
    internal partial class NvMapDeviceFile : NvDeviceFile<NvMapDeviceFile>
    {
        private const int FlagNotFreedYet = 1;

        private static readonly NvMapIdDictionary _maps = new();

        public NvMapDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, ulong owner) : base(context, owner)
        {
        }

        public override NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvMapCustomMagic)
            {
                switch (command.Number)
                {
                    case 0x01:
                        result = CallIoctlMethod<NvMapCreate>(Create, arguments);
                        break;
                    case 0x03:
                        result = CallIoctlMethod<NvMapFromId>(FromId, arguments);
                        break;
                    case 0x04:
                        result = CallIoctlMethod<NvMapAlloc>(Alloc, arguments);
                        break;
                    case 0x05:
                        result = CallIoctlMethod<NvMapFree>(Free, arguments);
                        break;
                    case 0x09:
                        result = CallIoctlMethod<NvMapParam>(Param, arguments);
                        break;
                    case 0x0e:
                        result = CallIoctlMethod<NvMapGetId>(GetId, arguments);
                        break;
                    case 0x02:
                    case 0x06:
                    case 0x07:
                    case 0x08:
                    case 0x0a:
                    case 0x0c:
                    case 0x0d:
                    case 0x0f:
                    case 0x10:
                    case 0x11:
                        result = NvInternalResult.NotSupported;
                        break;
                }
            }

            return result;
        }

        private NvInternalResult Create(ref NvMapCreate arguments)
        {
            if (arguments.Size == 0)
            {
                LogInvalidSizeArgument(arguments.Size);

                return NvInternalResult.InvalidInput;
            }

            uint size = BitUtils.AlignUp(arguments.Size, (uint)MemoryManager.PageSize);

            arguments.Handle = CreateHandleFromMap(new NvMapHandle(size));

            LogCreatedMap(arguments.Handle, arguments.Size);

            return NvInternalResult.Success;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Invalid size 0x{size:x8}!")]
        private partial void LogInvalidSizeArgument(uint size);

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Created map {handle} with size 0x{size:x8}!")]
        private partial void LogCreatedMap(int handle, uint size);

        private NvInternalResult FromId(ref NvMapFromId arguments)
        {
            NvMapHandle map = GetMapFromHandle(Owner, arguments.Id);

            if (map == null)
            {
                LogInvalidHandleArgument(arguments.Handle);

                return NvInternalResult.InvalidInput;
            }

            map.IncrementRefCount();

            arguments.Handle = arguments.Id;

            return NvInternalResult.Success;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Invalid handle 0x{handle:x8}!")]
        private partial void LogInvalidHandleArgument(int handle);

        private NvInternalResult Alloc(ref NvMapAlloc arguments)
        {
            NvMapHandle map = GetMapFromHandle(Owner, arguments.Handle);

            if (map == null)
            {
                LogInvalidHandleArgument(arguments.Handle);

                return NvInternalResult.InvalidInput;
            }

            if ((arguments.Align & (arguments.Align - 1)) != 0)
            {
                LogInvalidAlignment(arguments.Align);

                return NvInternalResult.InvalidInput;
            }

            if ((uint)arguments.Align < MemoryManager.PageSize)
            {
                arguments.Align = (int)MemoryManager.PageSize;
            }

            NvInternalResult result = NvInternalResult.Success;

            if (!map.Allocated)
            {
                map.Allocated = true;

                map.Align = arguments.Align;
                map.Kind = (byte)arguments.Kind;

                uint size = BitUtils.AlignUp(map.Size, (uint)MemoryManager.PageSize);

                ulong address = arguments.Address;

                if (address == 0)
                {
                    // When the address is zero, we need to allocate
                    // our own backing memory for the NvMap.
                    // TODO: Is this allocation inside the transfer memory?
                    result = NvInternalResult.OutOfMemory;
                }

                if (result == NvInternalResult.Success)
                {
                    map.Size = size;
                    map.Address = address;
                }
            }

            return result;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Invalid alignment 0x{align:x8}!")]
        private partial void LogInvalidAlignment(int align);

        private NvInternalResult Free(ref NvMapFree arguments)
        {
            NvMapHandle map = GetMapFromHandle(Owner, arguments.Handle);

            if (map == null)
            {
                LogInvalidHandleArgument(arguments.Handle);

                return NvInternalResult.InvalidInput;
            }

            if (DecrementMapRefCount(Owner, arguments.Handle))
            {
                arguments.Address = map.Address;
                arguments.Flags = 0;
            }
            else
            {
                arguments.Address = 0;
                arguments.Flags = FlagNotFreedYet;
            }

            arguments.Size = map.Size;

            return NvInternalResult.Success;
        }

        private NvInternalResult Param(ref NvMapParam arguments)
        {
            NvMapHandle map = GetMapFromHandle(Owner, arguments.Handle);

            if (map == null)
            {
                LogInvalidHandleArgument(arguments.Handle);

                return NvInternalResult.InvalidInput;
            }

            switch (arguments.Param)
            {
                case NvMapHandleParam.Size:
                    arguments.Result = (int)map.Size;
                    break;
                case NvMapHandleParam.Align:
                    arguments.Result = map.Align;
                    break;
                case NvMapHandleParam.Heap:
                    arguments.Result = 0x40000000;
                    break;
                case NvMapHandleParam.Kind:
                    arguments.Result = map.Kind;
                    break;
                case NvMapHandleParam.Compr:
                    arguments.Result = 0;
                    break;

                // Note: Base is not supported and returns an error.
                // Any other value also returns an error.
                default:
                    return NvInternalResult.InvalidInput;
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult GetId(ref NvMapGetId arguments)
        {
            NvMapHandle map = GetMapFromHandle(Owner, arguments.Handle);

            if (map == null)
            {
                LogInvalidHandleArgument(arguments.Handle);

                return NvInternalResult.InvalidInput;
            }

            arguments.Id = arguments.Handle;

            return NvInternalResult.Success;
        }

        public override void Close()
        {
            // TODO: refcount NvMapDeviceFile instances and remove when closing
            // _maps.TryRemove(GetOwner(), out _);
        }

        private int CreateHandleFromMap(NvMapHandle map)
        {
            return _maps.Add(map);
        }

        private static bool DeleteMapWithHandle(ulong pid, int handle)
        {
            return _maps.Delete(handle) != null;
        }

        public static void IncrementMapRefCount(ulong pid, int handle)
        {
            GetMapFromHandle(pid, handle)?.IncrementRefCount();
        }

        public static bool DecrementMapRefCount(ulong pid, int handle)
        {
            NvMapHandle map = GetMapFromHandle(pid, handle);

            if (map == null)
            {
                return false;
            }

            if (map.DecrementRefCount() <= 0)
            {
                DeleteMapWithHandle(pid, handle);

                LogDeletedMap(_logger, handle);

                return true;
            }
            else
            {
                return false;
            }
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Deleted map {handle}!")]
        private static partial void LogDeletedMap(ILogger logger, int handle);

        public static NvMapHandle GetMapFromHandle(ulong pid, int handle)
        {
            return _maps.Get(handle);
        }
    }
}