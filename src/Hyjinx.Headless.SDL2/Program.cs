using CommandLine;
using LibHac.Tools.FsSystem;
using Hyjinx.Audio.Backends.SDL2;
using Hyjinx.Common;
using Hyjinx.Common.Configuration;
using Hyjinx.Common.Configuration.Hid;
using Hyjinx.Common.Configuration.Hid.Controller;
using Hyjinx.Common.Configuration.Hid.Controller.Motion;
using Hyjinx.Common.Configuration.Hid.Keyboard;
using Hyjinx.Common.GraphicsDriver;
using Hyjinx.Common.Logging;
using Hyjinx.Common.SystemInterop;
using Hyjinx.Common.Utilities;
using Hyjinx.Cpu;
using Hyjinx.Graphics.GAL;
using Hyjinx.Graphics.GAL.Multithreading;
using Hyjinx.Graphics.Gpu;
using Hyjinx.Graphics.Gpu.Shader;
using Hyjinx.Graphics.OpenGL;
using Hyjinx.Graphics.Vulkan;
using Hyjinx.Graphics.Vulkan.MoltenVK;
using Hyjinx.Headless.SDL2.OpenGL;
using Hyjinx.Headless.SDL2.Vulkan;
using Hyjinx.HLE;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Account.Acc;
using Hyjinx.Input;
using Hyjinx.Input.HLE;
using Hyjinx.Input.SDL2;
using Hyjinx.SDL2.Common;
using Microsoft.Extensions.Logging;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using ConfigGamepadInputId = Hyjinx.Common.Configuration.Hid.Controller.GamepadInputId;
using ConfigStickInputId = Hyjinx.Common.Configuration.Hid.Controller.StickInputId;
using Key = Hyjinx.Common.Configuration.Hid.Key;
using LogLevel = Hyjinx.Common.Logging.LogLevel;

namespace Hyjinx.Headless.SDL2
{
    class Program
    {
        public static string Version { get; private set; }

        private static VirtualFileSystem _virtualFileSystem;
        private static ContentManager _contentManager;
        private static AccountManager _accountManager;
        private static LibHacHorizonManager _libHacHorizonManager;
        private static UserChannelPersistence _userChannelPersistence;
        private static InputManager _inputManager;
        private static Switch _emulationContext;
        private static WindowBase _window;
        private static WindowsMultimediaTimerResolution _windowsMultimediaTimerResolution;
        private static List<InputConfig> _inputConfiguration;
        private static bool _enableKeyboard;
        private static bool _enableMouse;

        static void Main(string[] args)
        {
            Version = ReleaseInformation.Version;

            // Make process DPI aware for proper window sizing on high-res screens.
            ForceDpiAware.Windows();

            Console.Title = $"Hyjinx Console {Version} (Headless SDL2)";

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                AutoResetEvent invoked = new(false);

                // MacOS must perform SDL polls from the main thread.
                SDL2Driver.MainThreadDispatcher = action =>
                {
                    invoked.Reset();

                    WindowBase.QueueMainThreadAction(() =>
                    {
                        action();

                        invoked.Set();
                    });

                    invoked.WaitOne();
                };
            }

            if (OperatingSystem.IsMacOS())
            {
                MVKInitialization.InitializeResolver();
            }

            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(Load)
            .WithNotParsed(errors => errors.Output());
        }

        private static InputConfig HandlePlayerConfiguration(string inputProfileName, string inputId, PlayerIndex index)
        {
            if (inputId == null)
            {
                if (index == PlayerIndex.Player1)
                {
                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                        "{index} not configured, defaulting to default keyboard.", index);

                    // Default to keyboard
                    inputId = "0";
                }
                else
                {
                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                        "{index} not configured", index);

                    return null;
                }
            }

            IGamepad gamepad;

            bool isKeyboard = true;

            gamepad = _inputManager.KeyboardDriver.GetGamepad(inputId);

            if (gamepad == null)
            {
                gamepad = _inputManager.GamepadDriver.GetGamepad(inputId);
                isKeyboard = false;

                if (gamepad == null)
                {
                    Logger.DefaultLogger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                        "{index} gamepad not found ('{inputId}')", index, inputId);

                    return null;
                }
            }

            string gamepadName = gamepad.Name;

            gamepad.Dispose();

            InputConfig config;

            if (inputProfileName == null || inputProfileName.Equals("default"))
            {
                if (isKeyboard)
                {
                    config = new StandardKeyboardInputConfig
                    {
                        Version = InputConfig.CurrentVersion,
                        Backend = InputBackendType.WindowKeyboard,
                        Id = null,
                        ControllerType = ControllerType.JoyconPair,
                        LeftJoycon = new LeftJoyconCommonConfig<Key>
                        {
                            DpadUp = Key.Up,
                            DpadDown = Key.Down,
                            DpadLeft = Key.Left,
                            DpadRight = Key.Right,
                            ButtonMinus = Key.Minus,
                            ButtonL = Key.E,
                            ButtonZl = Key.Q,
                            ButtonSl = Key.Unbound,
                            ButtonSr = Key.Unbound,
                        },

                        LeftJoyconStick = new JoyconConfigKeyboardStick<Key>
                        {
                            StickUp = Key.W,
                            StickDown = Key.S,
                            StickLeft = Key.A,
                            StickRight = Key.D,
                            StickButton = Key.F,
                        },

                        RightJoycon = new RightJoyconCommonConfig<Key>
                        {
                            ButtonA = Key.Z,
                            ButtonB = Key.X,
                            ButtonX = Key.C,
                            ButtonY = Key.V,
                            ButtonPlus = Key.Plus,
                            ButtonR = Key.U,
                            ButtonZr = Key.O,
                            ButtonSl = Key.Unbound,
                            ButtonSr = Key.Unbound,
                        },

                        RightJoyconStick = new JoyconConfigKeyboardStick<Key>
                        {
                            StickUp = Key.I,
                            StickDown = Key.K,
                            StickLeft = Key.J,
                            StickRight = Key.L,
                            StickButton = Key.H,
                        },
                    };
                }
                else
                {
                    bool isNintendoStyle = gamepadName.Contains("Nintendo");

                    config = new StandardControllerInputConfig
                    {
                        Version = InputConfig.CurrentVersion,
                        Backend = InputBackendType.GamepadSDL2,
                        Id = null,
                        ControllerType = ControllerType.JoyconPair,
                        DeadzoneLeft = 0.1f,
                        DeadzoneRight = 0.1f,
                        RangeLeft = 1.0f,
                        RangeRight = 1.0f,
                        TriggerThreshold = 0.5f,
                        LeftJoycon = new LeftJoyconCommonConfig<ConfigGamepadInputId>
                        {
                            DpadUp = ConfigGamepadInputId.DpadUp,
                            DpadDown = ConfigGamepadInputId.DpadDown,
                            DpadLeft = ConfigGamepadInputId.DpadLeft,
                            DpadRight = ConfigGamepadInputId.DpadRight,
                            ButtonMinus = ConfigGamepadInputId.Minus,
                            ButtonL = ConfigGamepadInputId.LeftShoulder,
                            ButtonZl = ConfigGamepadInputId.LeftTrigger,
                            ButtonSl = ConfigGamepadInputId.Unbound,
                            ButtonSr = ConfigGamepadInputId.Unbound,
                        },

                        LeftJoyconStick = new JoyconConfigControllerStick<ConfigGamepadInputId, ConfigStickInputId>
                        {
                            Joystick = ConfigStickInputId.Left,
                            StickButton = ConfigGamepadInputId.LeftStick,
                            InvertStickX = false,
                            InvertStickY = false,
                            Rotate90CW = false,
                        },

                        RightJoycon = new RightJoyconCommonConfig<ConfigGamepadInputId>
                        {
                            ButtonA = isNintendoStyle ? ConfigGamepadInputId.A : ConfigGamepadInputId.B,
                            ButtonB = isNintendoStyle ? ConfigGamepadInputId.B : ConfigGamepadInputId.A,
                            ButtonX = isNintendoStyle ? ConfigGamepadInputId.X : ConfigGamepadInputId.Y,
                            ButtonY = isNintendoStyle ? ConfigGamepadInputId.Y : ConfigGamepadInputId.X,
                            ButtonPlus = ConfigGamepadInputId.Plus,
                            ButtonR = ConfigGamepadInputId.RightShoulder,
                            ButtonZr = ConfigGamepadInputId.RightTrigger,
                            ButtonSl = ConfigGamepadInputId.Unbound,
                            ButtonSr = ConfigGamepadInputId.Unbound,
                        },

                        RightJoyconStick = new JoyconConfigControllerStick<ConfigGamepadInputId, ConfigStickInputId>
                        {
                            Joystick = ConfigStickInputId.Right,
                            StickButton = ConfigGamepadInputId.RightStick,
                            InvertStickX = false,
                            InvertStickY = false,
                            Rotate90CW = false,
                        },

                        Motion = new StandardMotionConfigController
                        {
                            MotionBackend = MotionInputBackendType.GamepadDriver,
                            EnableMotion = true,
                            Sensitivity = 100,
                            GyroDeadzone = 1,
                        },
                        Rumble = new RumbleConfigController
                        {
                            StrongRumble = 1f,
                            WeakRumble = 1f,
                            EnableRumble = false,
                        },
                    };
                }
            }
            else
            {
                string profileBasePath;

                if (isKeyboard)
                {
                    profileBasePath = Path.Combine(AppDataManager.ProfilesDirPath, "keyboard");
                }
                else
                {
                    profileBasePath = Path.Combine(AppDataManager.ProfilesDirPath, "controller");
                }

                string path = Path.Combine(profileBasePath, inputProfileName + ".json");

                if (!File.Exists(path))
                {
                    Logger.DefaultLogger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                        "Input profile '{inputProfileName}' not found for '{inputId}'", inputProfileName, inputId);

                    return null;
                }

                try
                {
                    // config = JsonHelper.DeserializeFromFile(path, _serializerContext.InputConfig);
                    throw new NotImplementedException();
                }
                catch (JsonException)
                {
                    Logger.DefaultLogger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                        "Input profile '{inputProfileName}' parsing failed for '{inputId}'", inputProfileName, inputId);

                    return null;
                }
            }

            config.Id = inputId;
            config.PlayerIndex = index;

            string inputTypeName = isKeyboard ? "Keyboard" : "Gamepad";

            Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                "{playerIndex} configured with {inputTypeName} \"{configId}\"", config.PlayerIndex, inputTypeName, config.Id);

            // If both stick ranges are 0 (usually indicative of an outdated profile load) then both sticks will be set to 1.0.
            if (config is StandardControllerInputConfig controllerConfig)
            {
                if (controllerConfig.RangeLeft <= 0.0f && controllerConfig.RangeRight <= 0.0f)
                {
                    controllerConfig.RangeLeft = 1.0f;
                    controllerConfig.RangeRight = 1.0f;
                    
                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                        "{playerIndex} stick range reset. Save the profile now to update your configuration", config.PlayerIndex);
                }
            }

            return config;
        }

        static void Load(Options option)
        {
            AppDataManager.Initialize(option.BaseDataDir);

            _virtualFileSystem = VirtualFileSystem.CreateInstance();
            _libHacHorizonManager = new LibHacHorizonManager();

            _libHacHorizonManager.InitializeFsServer(_virtualFileSystem);
            _libHacHorizonManager.InitializeArpServer();
            _libHacHorizonManager.InitializeBcatServer();
            _libHacHorizonManager.InitializeSystemClients();

            _contentManager = new ContentManager(_virtualFileSystem);
            _accountManager = new AccountManager(_libHacHorizonManager.HyjinxClient, option.UserProfile);
            _userChannelPersistence = new UserChannelPersistence();

            _inputManager = new InputManager(new SDL2KeyboardDriver(), new SDL2GamepadDriver());

            GraphicsConfig.EnableShaderCache = true;

            if (OperatingSystem.IsMacOS())
            {
                if (option.GraphicsBackend == GraphicsBackend.OpenGl)
                {
                    option.GraphicsBackend = GraphicsBackend.Vulkan;
                    Logger.Warning?.Print(LogClass.Application, "OpenGL is not supported on macOS, switching to Vulkan!");
                }
            }

            IGamepad gamepad;

            if (option.ListInputIds)
            {
                Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                    "Input Ids:");

                foreach (string id in _inputManager.KeyboardDriver.GamepadsIds)
                {
                    gamepad = _inputManager.KeyboardDriver.GetGamepad(id);

                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                        "- {id} (\"{gamepad}\")", id, gamepad.Name);

                    gamepad.Dispose();
                }

                foreach (string id in _inputManager.GamepadDriver.GamepadsIds)
                {
                    gamepad = _inputManager.GamepadDriver.GetGamepad(id);

                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                        "- {id} (\"{gamepad}\")", id, gamepad.Name);

                    gamepad.Dispose();
                }

                return;
            }

            if (option.InputPath == null)
            {
                Logger.DefaultLogger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                    "Please provide a file to load");

                return;
            }

            _inputConfiguration = new List<InputConfig>();
            _enableKeyboard = option.EnableKeyboard;
            _enableMouse = option.EnableMouse;

            static void LoadPlayerConfiguration(string inputProfileName, string inputId, PlayerIndex index)
            {
                InputConfig inputConfig = HandlePlayerConfiguration(inputProfileName, inputId, index);

                if (inputConfig != null)
                {
                    _inputConfiguration.Add(inputConfig);
                }
            }

            LoadPlayerConfiguration(option.InputProfile1Name, option.InputId1, PlayerIndex.Player1);
            LoadPlayerConfiguration(option.InputProfile2Name, option.InputId2, PlayerIndex.Player2);
            LoadPlayerConfiguration(option.InputProfile3Name, option.InputId3, PlayerIndex.Player3);
            LoadPlayerConfiguration(option.InputProfile4Name, option.InputId4, PlayerIndex.Player4);
            LoadPlayerConfiguration(option.InputProfile5Name, option.InputId5, PlayerIndex.Player5);
            LoadPlayerConfiguration(option.InputProfile6Name, option.InputId6, PlayerIndex.Player6);
            LoadPlayerConfiguration(option.InputProfile7Name, option.InputId7, PlayerIndex.Player7);
            LoadPlayerConfiguration(option.InputProfile8Name, option.InputId8, PlayerIndex.Player8);
            LoadPlayerConfiguration(option.InputProfileHandheldName, option.InputIdHandheld, PlayerIndex.Handheld);

            if (_inputConfiguration.Count == 0)
            {
                return;
            }

            // Setup logging level
            Logger.SetEnable(LogLevel.Debug, option.LoggingEnableDebug);
            Logger.SetEnable(LogLevel.Stub, !option.LoggingDisableStub);
            // Logger.SetEnable(LogLevel.Info, !option.LoggingDisableInfo);
            Logger.SetEnable(LogLevel.Warning, !option.LoggingDisableWarning);
            // Logger.SetEnable(LogLevel.Error, option.LoggingEnableError);
            Logger.SetEnable(LogLevel.Trace, option.LoggingEnableTrace);

            if (!option.DisableFileLog)
            {
                string logDir = AppDataManager.LogsDirPath;
                // FileStream logFile = null;
                
                // TODO: Fix this.
                // if (!string.IsNullOrEmpty(logDir))
                // {
                //     logFile = FileLogTarget.PrepareLogFile(logDir);
                // }
                //
                // if (logFile != null)
                // {
                //     Logger.AddTarget(new AsyncLogTargetWrapper(
                //         new FileLogTarget("file", logFile),
                //         1000,
                //         AsyncLogTargetOverflowAction.Block
                //     ));
                // }
                // else
                // {
                //     Logger.Error?.Print(LogClass.Application, "No writable log directory available. Make sure either the Logs directory, Application Data, or the Hyjinx directory is writable.");
                // }
            }

            // Setup graphics configuration
            GraphicsConfig.EnableShaderCache = !option.DisableShaderCache;
            GraphicsConfig.EnableTextureRecompression = option.EnableTextureRecompression;
            GraphicsConfig.ResScale = option.ResScale;
            GraphicsConfig.MaxAnisotropy = option.MaxAnisotropy;
            GraphicsConfig.ShadersDumpPath = option.GraphicsShadersDumpPath;
            GraphicsConfig.EnableMacroHLE = !option.DisableMacroHLE;

            DriverUtilities.InitDriverConfig(option.BackendThreading == BackendThreading.Off);

            while (true)
            {
                LoadApplication(option);

                if (_userChannelPersistence.PreviousIndex == -1 || !_userChannelPersistence.ShouldRestart)
                {
                    break;
                }

                _userChannelPersistence.ShouldRestart = false;
            }

            _inputManager.Dispose();
        }

        private static void SetupProgressHandler()
        {
            if (_emulationContext.Processes.ActiveApplication.DiskCacheLoadState != null)
            {
                _emulationContext.Processes.ActiveApplication.DiskCacheLoadState.StateChanged -= ProgressHandler;
                _emulationContext.Processes.ActiveApplication.DiskCacheLoadState.StateChanged += ProgressHandler;
            }

            _emulationContext.Gpu.ShaderCacheStateChanged -= ProgressHandler;
            _emulationContext.Gpu.ShaderCacheStateChanged += ProgressHandler;
        }

        private static void ProgressHandler<T>(T state, int current, int total) where T : Enum
        {
            string label = state switch
            {
                LoadState => $"PTC : {current}/{total}",
                ShaderCacheState => $"Shaders : {current}/{total}",
                _ => throw new ArgumentException($"Unknown Progress Handler type {typeof(T)}"),
            };

            Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), label);
        }

        private static WindowBase CreateWindow(Options options)
        {
            return options.GraphicsBackend == GraphicsBackend.Vulkan
                ? new VulkanWindow(_inputManager, options.LoggingGraphicsDebugLevel, options.AspectRatio, options.EnableMouse, options.HideCursorMode)
                : new OpenGLWindow(_inputManager, options.LoggingGraphicsDebugLevel, options.AspectRatio, options.EnableMouse, options.HideCursorMode);
        }

        private static IRenderer CreateRenderer(Options options, WindowBase window)
        {
            if (options.GraphicsBackend == GraphicsBackend.Vulkan && window is VulkanWindow vulkanWindow)
            {
                string preferredGpuId = string.Empty;
                Vk api = Vk.GetApi();

                if (!string.IsNullOrEmpty(options.PreferredGPUVendor))
                {
                    string preferredGpuVendor = options.PreferredGPUVendor.ToLowerInvariant();
                    var devices = VulkanRenderer.GetPhysicalDevices(api);

                    foreach (var device in devices)
                    {
                        if (device.Vendor.ToLowerInvariant() == preferredGpuVendor)
                        {
                            preferredGpuId = device.Id;
                            break;
                        }
                    }
                }

                return new VulkanRenderer(
                    api,
                    (instance, vk) => new SurfaceKHR((ulong)(vulkanWindow.CreateWindowSurface(instance.Handle))),
                    vulkanWindow.GetRequiredInstanceExtensions,
                    preferredGpuId);
            }

            return new OpenGLRenderer();
        }

        private static Switch InitializeEmulationContext(WindowBase window, IRenderer renderer, Options options)
        {
            BackendThreading threadingMode = options.BackendThreading;

            bool threadedGAL = threadingMode == BackendThreading.On || (threadingMode == BackendThreading.Auto && renderer.PreferThreading);

            if (threadedGAL)
            {
                renderer = new ThreadedRenderer(renderer);
            }

            HLEConfiguration configuration = new(_virtualFileSystem,
                _libHacHorizonManager,
                _contentManager,
                _accountManager,
                _userChannelPersistence,
                renderer,
                new SDL2HardwareDeviceDriver(),
                options.ExpandRAM ? MemoryConfiguration.MemoryConfiguration8GiB : MemoryConfiguration.MemoryConfiguration4GiB,
                window,
                options.SystemLanguage,
                options.SystemRegion,
                !options.DisableVSync,
                !options.DisableDockedMode,
                !options.DisablePTC,
                options.EnableInternetAccess,
                !options.DisableFsIntegrityChecks ? IntegrityCheckLevel.ErrorOnInvalid : IntegrityCheckLevel.None,
                options.FsGlobalAccessLogMode,
                options.SystemTimeOffset,
                options.SystemTimeZone,
                options.MemoryManagerMode,
                options.IgnoreMissingServices,
                options.AspectRatio,
                options.AudioVolume,
                options.UseHypervisor ?? true,
                options.MultiplayerLanInterfaceId,
                Hyjinx.Common.Configuration.Multiplayer.MultiplayerMode.Disabled);

            return new Switch(configuration);
        }

        private static void ExecutionEntrypoint()
        {
            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution = new WindowsMultimediaTimerResolution(1);
            }

            DisplaySleep.Prevent();

            _window.Initialize(_emulationContext, _inputConfiguration, _enableKeyboard, _enableMouse);

            _window.Execute();

            _emulationContext.Dispose();
            _window.Dispose();

            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution?.Dispose();
                _windowsMultimediaTimerResolution = null;
            }
        }

        private static bool LoadApplication(Options options)
        {
            string path = options.InputPath;

            WindowBase window = CreateWindow(options);
            IRenderer renderer = CreateRenderer(options, window);

            _window = window;

            _window.IsFullscreen = options.IsFullscreen;
            _window.DisplayId = options.DisplayId;
            _window.IsExclusiveFullscreen = options.IsExclusiveFullscreen;
            _window.ExclusiveFullscreenWidth = options.ExclusiveFullscreenWidth;
            _window.ExclusiveFullscreenHeight = options.ExclusiveFullscreenHeight;
            _window.AntiAliasing = options.AntiAliasing;
            _window.ScalingFilter = options.ScalingFilter;
            _window.ScalingFilterLevel = options.ScalingFilterLevel;

            _emulationContext = InitializeEmulationContext(window, renderer, options);

            SystemVersion firmwareVersion = _contentManager.GetCurrentFirmwareVersion();

            Logger.DefaultLogger.LogCritical(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
             "Using Firmware Version: {VersionString}", firmwareVersion?.VersionString);

            if (Directory.Exists(path))
            {
                string[] romFsFiles = Directory.GetFiles(path, "*.istorage");

                if (romFsFiles.Length == 0)
                {
                    romFsFiles = Directory.GetFiles(path, "*.romfs");
                }

                if (romFsFiles.Length > 0)
                {
                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                        "Loading as cart with RomFS.");

                    if (!_emulationContext.LoadCart(path, romFsFiles[0]))
                    {
                        _emulationContext.Dispose();

                        return false;
                    }
                }
                else
                {
                    Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)),
                        "Loading as cart WITHOUT RomFS.");

                    if (!_emulationContext.LoadCart(path))
                    {
                        _emulationContext.Dispose();

                        return false;
                    }
                }
            }
            else if (File.Exists(path))
            {
                switch (Path.GetExtension(path).ToLowerInvariant())
                {
                    case ".xci":
                        Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as XCI.");

                        if (!_emulationContext.LoadXci(path))
                        {
                            _emulationContext.Dispose();

                            return false;
                        }
                        break;
                    case ".nca":
                        Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as NCA.");

                        if (!_emulationContext.LoadNca(path))
                        {
                            _emulationContext.Dispose();

                            return false;
                        }
                        break;
                    case ".nsp":
                    case ".pfs0":
                        Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as NSP.");

                        if (!_emulationContext.LoadNsp(path))
                        {
                            _emulationContext.Dispose();

                            return false;
                        }
                        break;
                    default:
                        Logger.DefaultLogger.LogInformation(new EventId((int)LogClass.Application, nameof(LogClass.Application)), "Loading as Homebrew.");
                        try
                        {
                            if (!_emulationContext.LoadProgram(path))
                            {
                                _emulationContext.Dispose();

                                return false;
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Logger.DefaultLogger.LogError(new EventId((int)LogClass.Application, nameof(LogClass.Application)), 
                                "The specified file is not supported by Hyjinx.");

                            _emulationContext.Dispose();

                            return false;
                        }
                        break;
                }
            }
            else
            {
                Logger.Warning?.Print(LogClass.Application, $"Couldn't load '{options.InputPath}'. Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");

                _emulationContext.Dispose();

                return false;
            }

            SetupProgressHandler();
            ExecutionEntrypoint();

            return true;
        }
    }
}
