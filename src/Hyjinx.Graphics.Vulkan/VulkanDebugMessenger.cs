using Hyjinx.Common.Utilities;
using Hyjinx.Graphics.GAL;
using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using System;
using System.Runtime.InteropServices;

namespace Hyjinx.Graphics.Vulkan;

partial class VulkanDebugMessenger : IDisposable
{
    private static readonly ILogger<VulkanDebugMessenger> _logger = Logger.DefaultLoggerFactory.CreateLogger<VulkanDebugMessenger>();
    private readonly Vk _api;
    private readonly Instance _instance;
    private readonly GraphicsDebugLevel _logLevel;
    private readonly ExtDebugUtils _debugUtils;
    private readonly DebugUtilsMessengerEXT? _debugUtilsMessenger;
    private bool _disposed;

    public VulkanDebugMessenger(Vk api, Instance instance, GraphicsDebugLevel logLevel)
    {
        _api = api;
        _instance = instance;
        _logLevel = logLevel;

        _api.TryGetInstanceExtension(instance, out _debugUtils);

        Result result = TryInitialize(out _debugUtilsMessenger);

        if (result != Result.Success)
        {
            LogDebugInitializationFailed(result);
        }
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.Gpu, EventName = nameof(LogClass.Gpu),
        Message = "Vulkan debug messenger initialization failed with error {result}")]
    private partial void LogDebugInitializationFailed(Result result);

    private Result TryInitialize(out DebugUtilsMessengerEXT? debugUtilsMessengerHandle)
    {
        debugUtilsMessengerHandle = null;

        if (_debugUtils != null && _logLevel != GraphicsDebugLevel.None)
        {
            var messageType = _logLevel switch
            {
                GraphicsDebugLevel.Error => DebugUtilsMessageTypeFlagsEXT.ValidationBitExt,
                GraphicsDebugLevel.Slowdowns => DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                                                DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                GraphicsDebugLevel.All => DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                                          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                _ => throw new ArgumentException($"Invalid log level \"{_logLevel}\"."),
            };

            var messageSeverity = _logLevel switch
            {
                GraphicsDebugLevel.Error => DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                GraphicsDebugLevel.Slowdowns => DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt |
                                                DebugUtilsMessageSeverityFlagsEXT.WarningBitExt,
                GraphicsDebugLevel.All => DebugUtilsMessageSeverityFlagsEXT.InfoBitExt |
                                          DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                          DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                          DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                _ => throw new ArgumentException($"Invalid log level \"{_logLevel}\"."),
            };

            var debugUtilsMessengerCreateInfo = new DebugUtilsMessengerCreateInfoEXT
            {
                SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageType = messageType,
                MessageSeverity = messageSeverity,
            };

            unsafe
            {
                debugUtilsMessengerCreateInfo.PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(UserCallback);
            }

            DebugUtilsMessengerEXT messengerHandle = default;

            Result result = _debugUtils.CreateDebugUtilsMessenger(_instance, SpanHelpers.AsReadOnlySpan(ref debugUtilsMessengerCreateInfo), ReadOnlySpan<AllocationCallbacks>.Empty, SpanHelpers.AsSpan(ref messengerHandle));

            if (result == Result.Success)
            {
                debugUtilsMessengerHandle = messengerHandle;
            }

            return result;
        }

        return Result.Success;
    }

    private unsafe static uint UserCallback(
        DebugUtilsMessageSeverityFlagsEXT messageSeverity,
        DebugUtilsMessageTypeFlagsEXT messageTypes,
        DebugUtilsMessengerCallbackDataEXT* pCallbackData,
        void* pUserData)
    {
        var msg = Marshal.PtrToStringAnsi((IntPtr)pCallbackData->PMessage);

        if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt))
        {
            _logger.LogError(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), msg);
        }
        else if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.WarningBitExt))
        {
            _logger.LogWarning(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), msg);
        }
        else if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.InfoBitExt))
        {
            _logger.LogInformation(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), msg);
        }
        else // if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt))
        {
            _logger.LogDebug(new EventId((int)LogClass.Gpu, nameof(LogClass.Gpu)), msg);
        }

        return 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_debugUtilsMessenger.HasValue)
            {
                _debugUtils.DestroyDebugUtilsMessenger(_instance, _debugUtilsMessenger.Value, Span<AllocationCallbacks>.Empty);
            }

            _disposed = true;
        }
    }
}