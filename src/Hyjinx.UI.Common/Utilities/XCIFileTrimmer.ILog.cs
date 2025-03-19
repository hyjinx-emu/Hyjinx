namespace Hyjinx.UI.Common.Utilities;

public sealed partial class XCIFileTrimmer
{
    public interface ILog
    {
        public void Write(LogType logType, string text);
        public void Progress(long current, long total, string text, bool complete);
    }
}
