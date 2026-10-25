#if IS_LEGACY_ENABLED

namespace Hyjinx.Ava.Emulation.Utilities;

public sealed partial class XCIFileTrimmer
{
    public interface ILog
    {
        public void Write(LogType logType, string text);
        public void Progress(long current, long total, string text, bool complete);
    }
}

#endif