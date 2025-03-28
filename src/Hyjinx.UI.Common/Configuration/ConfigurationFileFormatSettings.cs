using Hyjinx.HLE.Utilities;

namespace Hyjinx.UI.Common.Configuration
{
    internal static class ConfigurationFileFormatSettings
    {
        public static readonly ConfigurationJsonSerializerContext SerializerContext = new(JsonHelper.GetDefaultSerializerOptions());
    }
}
