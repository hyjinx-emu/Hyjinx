using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Applet;
using Hyjinx.Horizon.Sdk.Sf;

namespace Hyjinx.Horizon.Sdk.Audio.Detail
{
    interface IFinalOutputRecorderManager : IServiceObject
    {
        Result OpenFinalOutputRecorder(
            out IFinalOutputRecorder recorder,
            FinalOutputRecorderParameter parameter,
            int processHandle,
            out FinalOutputRecorderParameterInternal outParameter,
            AppletResourceUserId appletResourceId);
    }
}