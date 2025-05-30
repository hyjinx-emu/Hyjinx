using Hyjinx.Horizon.Common;
using Hyjinx.Horizon.Sdk.Sf;
using System;
using System.Buffers;

namespace Hyjinx.Horizon.Sdk.Audio.Detail;

interface IAudioRenderer : IServiceObject
{
    Result GetSampleRate(out int sampleRate);
    Result GetSampleCount(out int sampleCount);
    Result GetMixBufferCount(out int mixBufferCount);
    Result GetState(out int state);
    Result RequestUpdate(Memory<byte> output, Memory<byte> performanceOutput, ReadOnlySequence<byte> input);
    Result Start();
    Result Stop();
    Result QuerySystemEvent(out int eventHandle);
    Result SetRenderingTimeLimit(int percent);
    Result GetRenderingTimeLimit(out int percent);
    Result RequestUpdateAuto(Memory<byte> output, Memory<byte> performanceOutput, ReadOnlySequence<byte> input);
    Result ExecuteAudioRendererRendering();
    Result SetVoiceDropParameter(float voiceDropParameter);
    Result GetVoiceDropParameter(out float voiceDropParameter);
}