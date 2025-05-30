namespace Hyjinx.Audio.Renderer.Dsp.Command;

public enum CommandType : byte
{
    Invalid,
    PcmInt16DataSourceVersion1,
    PcmInt16DataSourceVersion2,
    PcmFloatDataSourceVersion1,
    PcmFloatDataSourceVersion2,
    AdpcmDataSourceVersion1,
    AdpcmDataSourceVersion2,
    Volume,
    VolumeRamp,
    BiquadFilter,
    Mix,
    MixRamp,
    MixRampGrouped,
    DepopPrepare,
    DepopForMixBuffers,
    Delay,
    Upsample,
    DownMixSurroundToStereo,
    AuxiliaryBuffer,
    DeviceSink,
    CircularBufferSink,
    Reverb,
    Reverb3d,
    Performance,
    ClearMixBuffer,
    CopyMixBuffer,
    LimiterVersion1,
    LimiterVersion2,
    MultiTapBiquadFilter,
    CaptureBuffer,
    Compressor,
    BiquadFilterAndMix,
    MultiTapBiquadFilterAndMix,
}