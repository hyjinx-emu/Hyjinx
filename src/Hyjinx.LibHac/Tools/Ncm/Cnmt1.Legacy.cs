using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using System.IO;
namespace LibHac.Tools.Ncm;

public class Cnmt1 : Cnmt
{
    public ulong TitleId { get; }
    public TitleVersion TitleVersion { get; }
    public ContentMetaType Type { get; }
    public byte FieldD { get; }
    public int TableOffset { get; }
    public int ContentEntryCount { get; }
    public int MetaEntryCount { get; }
    public ContentMetaAttribute ContentMetaAttributes { get; }

    public CnmtContentEntry[] ContentEntries { get; }
    public CnmtContentMetaEntry[] MetaEntries { get; }

    public ulong ApplicationTitleId { get; }
    public ulong PatchTitleId { get; }
    public TitleVersion MinimumSystemVersion { get; }
    public TitleVersion MinimumApplicationVersion { get; }

    public CnmtExtended ExtendedData { get; }

    public byte[] Hash { get; }

    public Cnmt1() { }

    public Cnmt1(Stream file)
    {
        using (var reader = new BinaryReader(file))
        {
            TitleId = reader.ReadUInt64();
            uint version = reader.ReadUInt32();
            Type = (ContentMetaType)reader.ReadByte();
            TitleVersion = new TitleVersion(version, Type < ContentMetaType.Application);
            FieldD = reader.ReadByte();
            TableOffset = reader.ReadUInt16();
            ContentEntryCount = reader.ReadUInt16();
            MetaEntryCount = reader.ReadUInt16();
            ContentMetaAttributes = (ContentMetaAttribute)reader.ReadByte();

            // Old, pre-release cnmt files don't have the "required system version" field.
            // Try to detect this by reading the padding after that field.
            // The old format usually contains hashes there.
            file.Position += 7;
            int padding = reader.ReadInt32();
            bool isOldCnmtFormat = padding != 0;

            switch (Type)
            {
                case ContentMetaType.Application:
                    ApplicationTitleId = TitleId;
                    PatchTitleId = reader.ReadUInt64();
                    MinimumSystemVersion = new TitleVersion(reader.ReadUInt32(), true);
                    break;
                case ContentMetaType.Patch:
                    ApplicationTitleId = reader.ReadUInt64();
                    MinimumSystemVersion = new TitleVersion(reader.ReadUInt32(), true);
                    break;
                case ContentMetaType.AddOnContent:
                    ApplicationTitleId = reader.ReadUInt64();
                    MinimumApplicationVersion = new TitleVersion(reader.ReadUInt32());
                    break;
            }

            int baseOffset = isOldCnmtFormat ? 0x18 : 0x20;
            file.Position = baseOffset + TableOffset;

            ContentEntries = new CnmtContentEntry[ContentEntryCount];
            MetaEntries = new CnmtContentMetaEntry[MetaEntryCount];

            for (int i = 0; i < ContentEntryCount; i++)
            {
                ContentEntries[i] = new CnmtContentEntry(reader);
            }

            for (int i = 0; i < MetaEntryCount; i++)
            {
                MetaEntries[i] = new CnmtContentMetaEntry(reader);
            }

            if (Type == ContentMetaType.Patch)
            {
                ExtendedData = new CnmtExtended(reader);
            }

            Hash = reader.ReadBytes(0x20);
        }
    }
}