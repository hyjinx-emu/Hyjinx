#if IS_LEGACY_ENABLED

using LibHac.FsSystem;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE.Loaders.Processes;

partial class ProcessLoader
{
    public Task<bool> LoadUnpackedNcaAsync(string exeFsDirPath, string? romFsPath = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LoadUnpackedNca(exeFsDirPath, romFsPath));
    }

    private bool LoadUnpackedNca(string exeFsDirPath, string? romFsPath = null)
    {
        ProcessResult processResult = new LocalFileSystem(exeFsDirPath).Load(device, romFsPath);

        if (processResult.ProcessId != 0 && _processesByPid.TryAdd(processResult.ProcessId, processResult))
        {
            if (processResult.Start(device))
            {
                _latestPid = processResult.ProcessId;

                return true;
            }
        }

        return false;
    }
}

#endif