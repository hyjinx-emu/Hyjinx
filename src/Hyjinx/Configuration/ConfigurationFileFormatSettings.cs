using Hyjinx.Ava.Emulation.Configuration;
using Hyjinx.HLE.Utilities;

namespace Hyjinx.Ava.Configuration;

internal static class ConfigurationFileFormatSettings
{
    public static readonly ConfigurationJsonSerializerContext SerializerContext = new(JsonHelper.GetDefaultSerializerOptions());
}