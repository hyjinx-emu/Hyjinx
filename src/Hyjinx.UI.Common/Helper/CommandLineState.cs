using Hyjinx.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace Hyjinx.UI.Common.Helper
{
    public static partial class CommandLineState
    {
        public static string[] Arguments { get; private set; }

        public static bool? OverrideDockedMode { get; private set; }
        public static bool? OverrideHardwareAcceleration { get; private set; }
        public static string OverrideGraphicsBackend { get; private set; }
        public static string OverrideHideCursor { get; private set; }
        public static string BaseDirPathArg { get; private set; }
        public static string Profile { get; private set; }
        public static string LaunchPathArg { get; private set; }
        public static string LaunchApplicationId { get; private set; }
        public static bool StartFullscreenArg { get; private set; }
        public static string OverrideConfigFile { get; private set; }

        public static void ParseArguments(string[] args)
        {
            // TODO: Viper - The logging during startup needs to be corrected.
            // var _logger = Logger.DefaultLoggerFactory.CreateLogger(typeof(CommandLineState));
            List<string> arguments = new();

            // Parse Arguments.
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];

                switch (arg)
                {
                    case "-r":
                    case "--root-data-dir":
                        if (i + 1 >= args.Length)
                        {
                            // LogInvalidOption(_logger, arg);
                            continue;
                        }

                        BaseDirPathArg = args[++i];

                        arguments.Add(arg);
                        arguments.Add(args[i]);
                        break;
                    case "-p":
                    case "--profile":
                        if (i + 1 >= args.Length)
                        {
                            // LogInvalidOption(_logger, arg);
                            continue;
                        }

                        Profile = args[++i];

                        arguments.Add(arg);
                        arguments.Add(args[i]);
                        break;
                    case "-f":
                    case "--fullscreen":
                        StartFullscreenArg = true;

                        arguments.Add(arg);
                        break;
                    case "-g":
                    case "--graphics-backend":
                        if (i + 1 >= args.Length)
                        {
                            // LogInvalidOption(_logger, arg);
                            continue;
                        }

                        OverrideGraphicsBackend = args[++i];
                        break;
                    case "-i":
                    case "--application-id":
                        LaunchApplicationId = args[++i];
                        break;
                    case "--docked-mode":
                        OverrideDockedMode = true;
                        break;
                    case "--handheld-mode":
                        OverrideDockedMode = false;
                        break;
                    case "--hide-cursor":
                        if (i + 1 >= args.Length)
                        {
                            // LogInvalidOption(_logger, arg);
                            continue;
                        }

                        OverrideHideCursor = args[++i];
                        break;
                    case "--software-gui":
                        OverrideHardwareAcceleration = false;
                        break;
                    case "-c":
                    case "--config":
                        if (i + 1 >= args.Length)
                        {
                            // LogInvalidOption(_logger, arg);
                            continue;
                        }

                        string configFile = args[++i];

                        if (Path.GetExtension(configFile).ToLower() != ".json")
                        {
                            // LogInvalidOption(_logger, arg);
                            continue;
                        }

                        OverrideConfigFile = configFile;

                        arguments.Add(arg);
                        arguments.Add(args[i]);
                        break;
                    default:
                        LaunchPathArg = arg;
                        break;
                }
            }

            Arguments = arguments.ToArray();
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
            Message = "Invalid option '{arg}'")]
        private static partial void LogInvalidOption(ILogger logger, string arg);
    }
}
