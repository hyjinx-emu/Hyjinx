using Hyjinx.Audio.Integration;
using Hyjinx.Cpu;
using Hyjinx.Horizon.Sdk.Account;
using Hyjinx.Horizon.Sdk.Fs;
using LibHac;

namespace Hyjinx.Horizon;

public readonly struct HorizonOptions
{
    public bool IgnoreMissingServices { get; }
    public bool ThrowOnInvalidCommandIds { get; }

    public HorizonClient BcatClient { get; }
    public IFsClient FsClient { get; }
    public IEmulatorAccountManager AccountManager { get; }
    public IHardwareDeviceDriver AudioDeviceDriver { get; }
    public ITickSource TickSource { get; }

    public HorizonOptions(
        bool ignoreMissingServices,
        HorizonClient bcatClient,
        IFsClient fsClient,
        IEmulatorAccountManager accountManager,
        IHardwareDeviceDriver audioDeviceDriver,
        ITickSource tickSource)
    {
        IgnoreMissingServices = ignoreMissingServices;
        ThrowOnInvalidCommandIds = true;
        BcatClient = bcatClient;
        FsClient = fsClient;
        AccountManager = accountManager;
        AudioDeviceDriver = audioDeviceDriver;
        TickSource = tickSource;
    }
}