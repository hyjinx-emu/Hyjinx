using System;

namespace LibHac.Tools.Fs;

/// <summary>
/// Contains extension methods for interacting with XCI archives.
/// </summary>
public static class XciExtensions
{
    public static string GetFileName(this XciPartitionType type)
    {
        switch (type)
        {
            case XciPartitionType.Update:
                return "update";
            case XciPartitionType.Normal:
                return "normal";
            case XciPartitionType.Secure:
                return "secure";
            case XciPartitionType.Logo:
                return "logo";
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}