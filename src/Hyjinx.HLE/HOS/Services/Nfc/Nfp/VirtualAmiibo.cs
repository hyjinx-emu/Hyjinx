using Hyjinx.Common.Configuration;
using Hyjinx.Common.Memory;
using Hyjinx.Cpu;
using Hyjinx.HLE.HOS.Services.Mii;
using Hyjinx.HLE.HOS.Services.Mii.Types;
using Hyjinx.HLE.HOS.Services.Nfc.Nfp.NfpManager;
using Hyjinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hyjinx.HLE.HOS.Services.Nfc.Nfp;

static class VirtualAmiibo
{
    private static uint _openedApplicationAreaId;

    private static readonly AmiiboJsonSerializerContext _serializerContext = AmiiboJsonSerializerContext.Default;

    public static byte[] GenerateUuid(string amiiboId, bool useRandomUuid)
    {
        if (useRandomUuid)
        {
            return GenerateRandomUuid();
        }

        VirtualAmiiboFile virtualAmiiboFile = LoadAmiiboFile(amiiboId);

        if (virtualAmiiboFile.TagUuid.Length == 0)
        {
            virtualAmiiboFile.TagUuid = GenerateRandomUuid();

            SaveAmiiboFile(virtualAmiiboFile);
        }

        return virtualAmiiboFile.TagUuid;
    }

    private static byte[] GenerateRandomUuid()
    {
        byte[] uuid = new byte[9];

        Random.Shared.NextBytes(uuid);

        uuid[3] = (byte)(0x88 ^ uuid[0] ^ uuid[1] ^ uuid[2]);
        uuid[8] = (byte)(uuid[3] ^ uuid[4] ^ uuid[5] ^ uuid[6]);

        return uuid;
    }

    public static CommonInfo GetCommonInfo(string amiiboId)
    {
        VirtualAmiiboFile amiiboFile = LoadAmiiboFile(amiiboId);

        return new CommonInfo()
        {
            LastWriteYear = (ushort)amiiboFile.LastWriteDate.Year,
            LastWriteMonth = (byte)amiiboFile.LastWriteDate.Month,
            LastWriteDay = (byte)amiiboFile.LastWriteDate.Day,
            WriteCounter = amiiboFile.WriteCounter,
            Version = 1,
            ApplicationAreaSize = AmiiboConstants.ApplicationAreaSize,
            Reserved = new Array52<byte>(),
        };
    }

    public static RegisterInfo GetRegisterInfo(ITickSource tickSource, string amiiboId, string nickname)
    {
        VirtualAmiiboFile amiiboFile = LoadAmiiboFile(amiiboId);

        UtilityImpl utilityImpl = new(tickSource);
        CharInfo charInfo = new();

        charInfo.SetFromStoreData(StoreData.BuildDefault(utilityImpl, 0));

        charInfo.Nickname = Nickname.FromString(nickname);

        RegisterInfo registerInfo = new()
        {
            MiiCharInfo = charInfo,
            FirstWriteYear = (ushort)amiiboFile.FirstWriteDate.Year,
            FirstWriteMonth = (byte)amiiboFile.FirstWriteDate.Month,
            FirstWriteDay = (byte)amiiboFile.FirstWriteDate.Day,
            FontRegion = 0,
            Reserved1 = new Array64<byte>(),
            Reserved2 = new Array58<byte>(),
        };
        "Hyjinx"u8.CopyTo(registerInfo.Nickname.AsSpan());

        return registerInfo;
    }

    public static bool OpenApplicationArea(string amiiboId, uint applicationAreaId)
    {
        VirtualAmiiboFile virtualAmiiboFile = LoadAmiiboFile(amiiboId);

        if (virtualAmiiboFile.ApplicationAreas.Exists(item => item.ApplicationAreaId == applicationAreaId))
        {
            _openedApplicationAreaId = applicationAreaId;

            return true;
        }

        return false;
    }

    public static byte[] GetApplicationArea(string amiiboId)
    {
        VirtualAmiiboFile virtualAmiiboFile = LoadAmiiboFile(amiiboId);

        foreach (VirtualAmiiboApplicationArea applicationArea in virtualAmiiboFile.ApplicationAreas)
        {
            if (applicationArea.ApplicationAreaId == _openedApplicationAreaId)
            {
                return applicationArea.ApplicationArea;
            }
        }

        return Array.Empty<byte>();
    }

    public static bool CreateApplicationArea(string amiiboId, uint applicationAreaId, byte[] applicationAreaData)
    {
        VirtualAmiiboFile virtualAmiiboFile = LoadAmiiboFile(amiiboId);

        if (virtualAmiiboFile.ApplicationAreas.Exists(item => item.ApplicationAreaId == applicationAreaId))
        {
            return false;
        }

        virtualAmiiboFile.ApplicationAreas.Add(new VirtualAmiiboApplicationArea()
        {
            ApplicationAreaId = applicationAreaId,
            ApplicationArea = applicationAreaData,
        });

        SaveAmiiboFile(virtualAmiiboFile);

        return true;
    }

    public static void SetApplicationArea(string amiiboId, byte[] applicationAreaData)
    {
        VirtualAmiiboFile virtualAmiiboFile = LoadAmiiboFile(amiiboId);

        if (virtualAmiiboFile.ApplicationAreas.Exists(item => item.ApplicationAreaId == _openedApplicationAreaId))
        {
            for (int i = 0; i < virtualAmiiboFile.ApplicationAreas.Count; i++)
            {
                if (virtualAmiiboFile.ApplicationAreas[i].ApplicationAreaId == _openedApplicationAreaId)
                {
                    virtualAmiiboFile.ApplicationAreas[i] = new VirtualAmiiboApplicationArea()
                    {
                        ApplicationAreaId = _openedApplicationAreaId,
                        ApplicationArea = applicationAreaData,
                    };

                    break;
                }
            }

            SaveAmiiboFile(virtualAmiiboFile);
        }
    }

    private static VirtualAmiiboFile LoadAmiiboFile(string amiiboId)
    {
        Directory.CreateDirectory(Path.Join(AppDataManager.KeysDirPath, "amiibo"));

        string filePath = Path.Join(AppDataManager.KeysDirPath, "amiibo", $"{amiiboId}.json");

        VirtualAmiiboFile virtualAmiiboFile;

        if (File.Exists(filePath))
        {
            virtualAmiiboFile = JsonHelper.DeserializeFromFile(filePath, _serializerContext.VirtualAmiiboFile);
        }
        else
        {
            virtualAmiiboFile = new VirtualAmiiboFile()
            {
                FileVersion = 0,
                TagUuid = Array.Empty<byte>(),
                AmiiboId = amiiboId,
                FirstWriteDate = DateTime.Now,
                LastWriteDate = DateTime.Now,
                WriteCounter = 0,
                ApplicationAreas = new List<VirtualAmiiboApplicationArea>(),
            };

            SaveAmiiboFile(virtualAmiiboFile);
        }

        return virtualAmiiboFile;
    }

    private static void SaveAmiiboFile(VirtualAmiiboFile virtualAmiiboFile)
    {
        string filePath = Path.Join(AppDataManager.KeysDirPath, "amiibo", $"{virtualAmiiboFile.AmiiboId}.json");
        JsonHelper.SerializeToFile(filePath, virtualAmiiboFile, _serializerContext.VirtualAmiiboFile);
    }
}