namespace Hyjinx.Audio.Backends.SoundIo.Native;

internal enum SoundIoBackend
{
    None = 0,
    Jack = 1,
    PulseAudio = 2,
    Alsa = 3,
    CoreAudio = 4,
    Wasapi = 5,
    Dummy = 6,
}