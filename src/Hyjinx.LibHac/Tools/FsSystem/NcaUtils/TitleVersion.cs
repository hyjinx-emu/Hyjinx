namespace LibHac.Tools.FsSystem.NcaUtils;

public record TitleVersion
{
    public uint Version { get; }
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public int Revision { get; }

    public TitleVersion(uint version, bool isSystemTitle = false)
    {
        Version = version;

        if (isSystemTitle)
        {
            Revision = (int)(version & ((1 << 16) - 1));
            Patch = (int)((version >> 16) & ((1 << 4) - 1));
            Minor = (int)((version >> 20) & ((1 << 6) - 1));
            Major = (int)((version >> 26) & ((1 << 6) - 1));
        }
        else
        {
            Revision = (byte)version;
            Patch = (byte)(version >> 8);
            Minor = (byte)(version >> 16);
            Major = (byte)(version >> 24);
        }
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}.{Revision}";
    }
}