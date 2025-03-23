namespace Hyjinx.UI.Common.Utilities;

public sealed partial class XCIFileTrimmer
{
    public enum OperationOutcome
    {
        InvalidXCIFile,
        NoTrimNecessary,
        NoUntrimPossible,
        FreeSpaceCheckFailed,
        FileIOWriteError,
        ReadOnlyFileCannotFix,
        FileSizeChanged,
        Successful
    }
}
