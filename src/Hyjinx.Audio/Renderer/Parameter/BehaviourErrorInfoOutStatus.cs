using Hyjinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using static Hyjinx.Audio.Renderer.Common.BehaviourParameter;

namespace Hyjinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for behaviour.
    /// </summary>
    /// <remarks>This is used to report errors to the user during <see cref="Server.AudioRenderSystem.Update(Memory{byte}, Memory{byte}, System.Buffers.ReadOnlySequence{byte})"/> processing.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BehaviourErrorInfoOutStatus
    {
        /// <summary>
        /// The reported errors.
        /// </summary>
        public Array10<ErrorInfo> ErrorInfos;

        /// <summary>
        /// The amount of error that got reported.
        /// </summary>
        public uint ErrorInfosCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}