using Hyjinx.Ava.Common.Locale;

namespace Hyjinx.Ava.UI.Models
{
    public class TitleUpdateModel
    {
        public uint Version { get; }
        public string Path { get; }
        public string Label { get; }

        public TitleUpdateModel(uint version, string displayVersion, string path)
        {
            Version = version;
            Label = LocaleManager.Instance.UpdateAndGetDynamicValue(
                System.IO.Path.GetExtension(path)?.ToLower() == ".xci" ? LocaleKeys.TitleBundledUpdateVersionLabel : LocaleKeys.TitleUpdateVersionLabel,
                displayVersion
            );
            Path = path;
        }
    }
}