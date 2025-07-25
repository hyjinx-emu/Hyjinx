using Hyjinx.Audio.Backends.CompatLayer;
using Hyjinx.Audio.Integration;
using Hyjinx.Graphics.Gpu;
using Hyjinx.HLE.FileSystem;
using Hyjinx.HLE.HOS;
using Hyjinx.HLE.HOS.Services.Apm;
using Hyjinx.HLE.HOS.Services.Hid;
using Hyjinx.HLE.Loaders.Processes;
using Hyjinx.HLE.UI;
using Hyjinx.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hyjinx.HLE;

public class Switch : IDisposable
{
    public HLEConfiguration Configuration { get; }
    public IHardwareDeviceDriver AudioDeviceDriver { get; }
    public MemoryBlock Memory { get; }
    public GpuContext Gpu { get; }
    public VirtualFileSystem FileSystem { get; }
    public HOS.Horizon System { get; }
    public ProcessLoader Processes { get; }
    public PerformanceStatistics Statistics { get; }
    public Hid Hid { get; }
    public TamperMachine TamperMachine { get; }
    public IHostUIHandler UIHandler { get; }

    public bool EnableDeviceVsync { get; set; }

    public bool IsFrameAvailable => Gpu.Window.IsFrameAvailable;

    public Switch(HLEConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration.GpuRenderer);
        ArgumentNullException.ThrowIfNull(configuration.AudioDeviceDriver);
        ArgumentNullException.ThrowIfNull(configuration.UserChannelPersistence);

        Configuration = configuration;
        FileSystem = Configuration.VirtualFileSystem;
        UIHandler = Configuration.HostUIHandler;

        MemoryAllocationFlags memoryAllocationFlags = configuration.MemoryManagerMode == MemoryManagerMode.SoftwarePageTable
            ? MemoryAllocationFlags.Reserve
            : MemoryAllocationFlags.Reserve | MemoryAllocationFlags.Mirrorable;

#pragma warning disable IDE0055 // Disable formatting
        AudioDeviceDriver = new CompatLayerHardwareDeviceDriver(Configuration.AudioDeviceDriver);
        Memory            = new MemoryBlock(Configuration.MemoryConfiguration.ToDramSize(), memoryAllocationFlags);
        Gpu               = new GpuContext(Configuration.GpuRenderer);
        System            = new HOS.Horizon(this);
        Statistics        = new PerformanceStatistics();
        Hid               = new Hid(this, System.HidStorage);
        Processes         = new ProcessLoader(this);
        TamperMachine     = new TamperMachine();

        System.InitializeServices();
        System.State.SetLanguage(Configuration.SystemLanguage);
        System.State.SetRegion(Configuration.Region);

        EnableDeviceVsync                       = Configuration.EnableVsync;
        System.State.DockedMode                 = Configuration.EnableDockedMode;
        System.PerformanceState.PerformanceMode = System.State.DockedMode ? PerformanceMode.Boost : PerformanceMode.Default;
        System.EnablePtc                        = Configuration.EnablePtc;
        System.FsIntegrityCheckLevel            = Configuration.FsIntegrityCheckLevel;
        System.GlobalAccessLogMode              = Configuration.FsGlobalAccessLogMode;
#pragma warning restore IDE0055
    }

    public async Task<bool> LoadCartAsync(string exeFsDir, string? romFsFile = null, CancellationToken cancellationToken = default)
    {
        return await Processes.LoadUnpackedNcaAsync(exeFsDir, romFsFile, cancellationToken);
    }

    public async Task<bool> LoadXciAsync(string xciFile, ulong applicationId = 0, CancellationToken cancellationToken = default)
    {
        return await Processes.LoadXciAsync(xciFile, applicationId, cancellationToken);
    }
    
    public async Task<bool> LoadNcaAsync(string ncaFile, CancellationToken cancellationToken = default)
    {
        return await Processes.LoadNcaAsync(ncaFile, cancellationToken);
    }

    public async Task<bool> LoadNspAsync(string nspFile, ulong applicationId = 0, CancellationToken cancellationToken = default)
    {
        return await Processes.LoadNspAsync(nspFile, applicationId, cancellationToken);
    }

    public async Task<bool> LoadProgramAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return await Processes.LoadNxoAsync(fileName, cancellationToken);
    }

    public bool WaitFifo()
    {
        return Gpu.GPFifo.WaitForCommands();
    }

    public void ProcessFrame()
    {
        Gpu.ProcessShaderCacheQueue();
        Gpu.Renderer.PreFrame();
        Gpu.GPFifo.DispatchCalls();
    }

    public bool ConsumeFrameAvailable()
    {
        return Gpu.Window.ConsumeFrameAvailable();
    }

    public void PresentFrame(Action swapBuffersCallback)
    {
        Gpu.Window.Present(swapBuffersCallback);
    }

    public void SetVolume(float volume)
    {
        AudioDeviceDriver.Volume = Math.Clamp(volume, 0f, 1f);
    }

    public float GetVolume()
    {
        return AudioDeviceDriver.Volume;
    }

    public void EnableCheats()
    {
        ModLoader.EnableCheats(Processes.ActiveApplication.ProgramId, TamperMachine);
    }

    public bool IsAudioMuted()
    {
        return AudioDeviceDriver.Volume == 0;
    }

    public void DisposeGpu()
    {
        Gpu.Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            System.Dispose();
            AudioDeviceDriver.Dispose();
            FileSystem.Dispose();
            Memory.Dispose();
        }
    }
}