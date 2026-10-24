using Hyjinx.HLE.Utilities;

namespace Hyjinx.Ava.Emulation.Configuration;

internal static class ConfigurationFileFormatSettings
{
    public static readonly ConfigurationJsonSerializerContext SerializerContext = new(JsonHelper.GetDefaultSerializerOptions());
}