#if IS_LEGACY_ENABLED

namespace Hyjinx.Ava.Emulation.Utilities;

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

#endif