#if IS_LEGACY_ENABLED

using Hyjinx.Common;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Kernel;
using Hyjinx.HLE.HOS.Kernel.Common;
using Hyjinx.HLE.HOS.Kernel.Memory;
using Hyjinx.HLE.HOS.Kernel.Process;
using Hyjinx.HLE.Loaders.Executables;
using Hyjinx.HLE.Loaders.Processes.Extensions;
using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Arp;
using Hyjinx.Logging.Abstractions;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Fs.Shim;
using LibHac.Loader;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using ApplicationId = LibHac.Ncm.ApplicationId;

namespace Hyjinx.HLE.Loaders.Processes;

partial class ProcessLoaderHelper
{
    public static LibHac.Result RegisterProgramMapInfo(Switch device, IFileSystem partitionFileSystem)
    {
        ulong applicationId = 0;
        int programCount = 0;

        Span<bool> hasIndex = stackalloc bool[0x10];

        foreach (DirectoryEntryEx fileEntry in partitionFileSystem.EnumerateEntries("/", "*.nca"))
        {
            Nca nca = partitionFileSystem.GetNca(device.FileSystem.KeySet, fileEntry.FullPath);

            if (!nca.IsProgram())
            {
                continue;
            }

            ulong currentMainProgramId = nca.GetProgramIdBase();

            if (applicationId == 0 && currentMainProgramId != 0)
            {
                applicationId = currentMainProgramId;
            }

            if (applicationId != currentMainProgramId)
            {
                // Currently there aren't any known multi-application game cards containing multi-program applications,
                // so because multi-application game cards are the only way we could run into multiple applications
                // we'll just return that there's a single program.
                programCount = 1;

                break;
            }

            hasIndex[nca.GetProgramIndex()] = true;
        }

        if (programCount == 0)
        {
            for (int i = 0; i < hasIndex.Length && hasIndex[i]; i++)
            {
                programCount++;
            }
        }

        if (programCount <= 0)
        {
            return LibHac.Result.Success;
        }

        Span<ProgramIndexMapInfo> mapInfo = stackalloc ProgramIndexMapInfo[0x10];

        for (int i = 0; i < programCount; i++)
        {
            mapInfo[i].ProgramId = new ProgramId(applicationId + (uint)i);
            mapInfo[i].MainProgramId = new ApplicationId(applicationId);
            mapInfo[i].ProgramIndex = (byte)i;
        }

        return device.System.LibHacHorizonManager.NsClient.Fs.RegisterProgramIndexMapInfo(mapInfo[..programCount]);
    }
}

#endif