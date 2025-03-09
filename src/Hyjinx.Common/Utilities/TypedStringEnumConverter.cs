using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hyjinx.Common.Utilities
{
    /// <summary>
    /// Specifies that value of <see cref="TEnum"/> will be serialized as string in JSONs
    /// </summary>
    /// <remarks>
    /// Trimming friendly alternative to <see cref="JsonStringEnumConverter"/>.
    /// Get rid of this converter if dotnet supports similar functionality out of the box.
    /// </remarks>
    /// <typeparam name="TEnum">Type of enum to serialize</typeparam>
    public sealed partial class TypedStringEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private static readonly ILogger<TypedStringEnumConverter<TEnum>> _logger =
            Logger.DefaultLoggerFactory.CreateLogger<TypedStringEnumConverter<TEnum>>();
        
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var enumValue = reader.GetString();

            if (Enum.TryParse(enumValue, out TEnum value))
            {
                return value;
            }

            LogFailedToParseEnumValue(enumValue!);
            return default;
        }

        [LoggerMessage(LogLevel.Warning,
            EventId = (int)LogClass.Configuration, EventName = nameof(LogClass.Configuration),
            Message = "Failed to parse enum value '{enumValue}', using default value instead.")]
        private partial void LogFailedToParseEnumValue(string enumValue);

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
