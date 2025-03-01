using Avalonia.Logging;
using Avalonia.Utilities;
using System;
using System.Text;

using AvaLogger = Avalonia.Logging.Logger;
using AvaLogLevel = Avalonia.Logging.LogEventLevel;
using AppLogClass = Hyjinx.Common.Logging.LogClass;
using AppLogger = Hyjinx.Common.Logging.Logger;

namespace Hyjinx.Ava.UI.Helpers
{
    internal class LoggerAdapter : ILogSink
    {
        public static void Register()
        {
            AvaLogger.Sink = new LoggerAdapter();
        }

        private static AppLogger.Log? GetLog(AvaLogLevel level)
        {
            return level switch
            {
                AvaLogLevel.Verbose => AppLogger.Debug,
                AvaLogLevel.Debug => AppLogger.Debug,
                AvaLogLevel.Information => AppLogger.Debug,
                AvaLogLevel.Warning => AppLogger.Debug,
                AvaLogLevel.Error => AppLogger.Error,
                AvaLogLevel.Fatal => AppLogger.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
            };
        }

        public bool IsEnabled(AvaLogLevel level, string area)
        {
            return GetLog(level) != null;
        }

        public void Log(AvaLogLevel level, string area, object source, string messageTemplate)
        {
            GetLog(level)?.PrintMsg(AppLogClass.UI, Format(level, area, messageTemplate, source, null));
        }

        public void Log(AvaLogLevel level, string area, object source, string messageTemplate, params object[] propertyValues)
        {
            GetLog(level)?.PrintMsg(AppLogClass.UI, Format(level, area, messageTemplate, source, propertyValues));
        }

        private static string Format(AvaLogLevel level, string area, string template, object source, object[] v)
        {
            var result = new StringBuilder();
            var r = new CharacterReader(template.AsSpan());
            int i = 0;

            result.Append('[');
            result.Append(level);
            result.Append("] ");

            result.Append('[');
            result.Append(area);
            result.Append("] ");

            while (!r.End)
            {
                var c = r.Take();

                if (c != '{')
                {
                    result.Append(c);
                }
                else
                {
                    if (r.Peek != '{')
                    {
                        result.Append('\'');
                        result.Append(i < v.Length ? v[i++] : null);
                        result.Append('\'');
                        r.TakeUntil('}');
                        r.Take();
                    }
                    else
                    {
                        result.Append('{');
                        r.Take();
                    }
                }
            }

            if (source != null)
            {
                result.Append(" (");
                result.Append(source.GetType().Name);
                result.Append(" #");
                result.Append(source.GetHashCode());
                result.Append(')');
            }

            return result.ToString();
        }
    }
}
