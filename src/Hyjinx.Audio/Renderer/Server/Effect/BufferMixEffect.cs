using Hyjinx.Audio.Renderer.Common;
using Hyjinx.Audio.Renderer.Parameter;
using Hyjinx.Audio.Renderer.Parameter.Effect;
using Hyjinx.Audio.Renderer.Server.MemoryPool;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hyjinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for a buffer mix effect.
    /// </summary>
    public class BufferMixEffect : BaseEffect
    {
        /// <summary>
        /// The buffer mix parameter.
        /// </summary>
        public BufferMixParameter Parameter;

        public override EffectType TargetEffectType => EffectType.BufferMix;

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, in EffectInParameterVersion1 parameter, PoolMapper mapper)
        {
            Update(out updateErrorInfo, in parameter, mapper);
        }

        public override void Update(out BehaviourParameter.ErrorInfo updateErrorInfo, in EffectInParameterVersion2 parameter, PoolMapper mapper)
        {
            Update(out updateErrorInfo, in parameter, mapper);
        }

        public void Update<T>(out BehaviourParameter.ErrorInfo updateErrorInfo, in T parameter, PoolMapper mapper) where T : unmanaged, IEffectInParameter
        {
            Debug.Assert(IsTypeValid(in parameter));

            UpdateParameterBase(in parameter);

            Parameter = MemoryMarshal.Cast<byte, BufferMixParameter>(parameter.SpecificData)[0];
            IsEnabled = parameter.IsEnabled;

            updateErrorInfo = new BehaviourParameter.ErrorInfo();
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();
        }
    }
}
