namespace LibHac.Tools.FsSystem.NcaUtils;

public enum NcaContentType : byte
{
    Program = 0,
    Meta = 1,
    Control = 2,
    Manual = 3,
    Data = 4,
    PublicData = 5
}