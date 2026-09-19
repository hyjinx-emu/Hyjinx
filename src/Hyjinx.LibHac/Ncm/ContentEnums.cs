using System;

namespace LibHac.Ncm;

public enum ContentType : byte
{
    Meta = 0,
    Program = 1,
    Data = 2,
    Control = 3,
    HtmlDocument = 4,
    LegalInformation = 5,
    DeltaFragment = 6
}

public enum ContentMetaType : byte
{
    Unknown = 0x0,
    SystemProgram = 0x1,
    SystemData = 0x2,
    SystemUpdate = 0x3,
    BootImagePackage = 0x4,
    BootImagePackageSafe = 0x5,
    Application = 0x80,
    Patch = 0x81,
    AddOnContent = 0x82,
    Delta = 0x83,
    DeltaPatch = 0x84
}

public enum ContentMetaPlatform : byte
{
    Nx = 0x0
}

public enum ContentInstallType : byte
{
    Full = 0,
    FragmentOnly = 1,
    Unknown = 7
}

[Flags]
public enum ContentMetaAttribute : byte
{
    None = 0,
    IncludesExFatDriver = 1 << 0,
    Rebootless = 1 << 1,
    Compacted = 1 << 2,
}

public enum UpdateType : byte
{
    ApplyAsDelta = 0,
    Overwrite = 1,
    Create = 2
}