using Hyjinx.Audio.Renderer.Server;

namespace Hyjinx.Tests.Audio.Renderer.Server
{
    public class BehaviourContextTests
    {
        [Test]
        public void TestCheckFeature()
        {
            int latestRevision = BehaviourContext.BaseRevisionMagic + BehaviourContext.LastRevision;
            int previousRevision = BehaviourContext.BaseRevisionMagic + (BehaviourContext.LastRevision - 1);
            int invalidRevision = BehaviourContext.BaseRevisionMagic + (BehaviourContext.LastRevision + 1);

            ClassicAssert.IsTrue(BehaviourContext.CheckFeatureSupported(latestRevision, latestRevision));
            ClassicAssert.IsFalse(BehaviourContext.CheckFeatureSupported(previousRevision, latestRevision));
            ClassicAssert.IsTrue(BehaviourContext.CheckFeatureSupported(latestRevision, previousRevision));
            // In case we get an invalid revision, this is supposed to auto default to REV1 internally.. idk what the hell Nintendo was thinking here..
            ClassicAssert.IsTrue(BehaviourContext.CheckFeatureSupported(invalidRevision, latestRevision));
        }

        [Test]
        public void TestsMemoryPoolForceMappingEnabled()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision1);

            ClassicAssert.IsFalse(behaviourContext.IsMemoryPoolForceMappingEnabled());

            behaviourContext.UpdateFlags(0x1);

            ClassicAssert.IsTrue(behaviourContext.IsMemoryPoolForceMappingEnabled());
        }

        [Test]
        public void TestRevision1()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision1);

            ClassicAssert.IsFalse(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsFalse(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsFalse(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsFalse(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsFalse(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.70f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision2()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision2);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsFalse(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsFalse(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsFalse(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsFalse(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.70f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision3()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision3);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsFalse(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsFalse(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsFalse(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.70f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision4()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision4);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsFalse(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsFalse(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.75f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision5()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision5);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(2, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision6()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision6);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsFalse(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(2, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision7()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision7);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsFalse(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(2, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision8()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision8);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsTrue(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(3, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision9()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision9);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsTrue(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsFalse(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(3, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision10()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision10);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsTrue(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsFalse(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(4, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision11()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision11);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsTrue(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsTrue(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsFalse(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(5, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision12()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision12);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsTrue(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsTrue(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsFalse(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(5, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision13()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision13);

            ClassicAssert.IsTrue(behaviourContext.IsAdpcmLoopContextBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterSupported());
            ClassicAssert.IsTrue(behaviourContext.IsLongSizePreDelaySupported());
            ClassicAssert.IsTrue(behaviourContext.IsAudioUsbDeviceOutputSupported());
            ClassicAssert.IsTrue(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsElapsedFrameCountSupported());
            ClassicAssert.IsTrue(behaviourContext.IsDecodingBehaviourFlagSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            ClassicAssert.IsTrue(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            ClassicAssert.IsTrue(behaviourContext.IsWaveBufferVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.IsEffectInfoVersion2Supported());
            ClassicAssert.IsTrue(behaviourContext.UseMultiTapBiquadFilterProcessing());
            ClassicAssert.IsTrue(behaviourContext.IsNewEffectChannelMappingSupported());
            ClassicAssert.IsTrue(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            ClassicAssert.IsTrue(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(5, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }
    }
}
