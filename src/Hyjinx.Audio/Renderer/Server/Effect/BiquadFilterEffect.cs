using Hyjinx.Audio.Renderer.Common;
using Hyjinx.Audio.Renderer.Dsp.State;
using Hyjinx.Audio.Renderer.Parameter;
using Hyjinx.Audio.Renderer.Parameter.Effect;
using Hyjinx.Audio.Renderer.Server.MemoryPool;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hyjinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// Server state for a biquad filter effect.
    /// </summary>
    public class BiquadFilterEffect : BaseEffect
    {
        /// <summary>
        /// The biquad filter parameter.
        /// </summary>
        public BiquadFilterEffectParameter Parameter;

        /// <summary>
        /// The biquad filter state.
        /// </summary>
        public Memory<BiquadFilterState> State { get; }

        /// <summary>
        /// Create a new <see cref="BiquadFilterEffect"/>.
        /// </summary>
        public BiquadFilterEffect()
        {
            Parameter = new BiquadFilterEffectParameter();
            State = new BiquadFilterState[Constants.ChannelCountMax];
        }

        public override EffectType TargetEffectType => EffectType.BiquadFilter;

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

            Parameter = MemoryMarshal.Cast<byte, BiquadFilterEffectParameter>(parameter.SpecificData)[0];
            IsEnabled = parameter.IsEnabled;

            updateErrorInfo = new BehaviourParameter.ErrorInfo();
        }

        public override void UpdateForCommandGeneration()
        {
            UpdateUsageStateForCommandGeneration();

            Parameter.Status = UsageState.Enabled;
        }
    }
}
