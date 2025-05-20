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

            Assert.That(BehaviourContext.CheckFeatureSupported(latestRevision, latestRevision));
            Assert.That(!BehaviourContext.CheckFeatureSupported(previousRevision, latestRevision));
            Assert.That(BehaviourContext.CheckFeatureSupported(latestRevision, previousRevision));
            // In case we get an invalid revision, this is supposed to auto default to REV1 internally.. idk what the hell Nintendo was thinking here..
            Assert.That(BehaviourContext.CheckFeatureSupported(invalidRevision, latestRevision));
        }

        [Test]
        public void TestsMemoryPoolForceMappingEnabled()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision1);

            Assert.That(!behaviourContext.IsMemoryPoolForceMappingEnabled());

            behaviourContext.UpdateFlags(0x1);

            Assert.That(behaviourContext.IsMemoryPoolForceMappingEnabled());
        }

        [Test]
        public void TestRevision1()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision1);

            Assert.That(!behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(!behaviourContext.IsSplitterSupported());
            Assert.That(!behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(!behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(!behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(!behaviourContext.IsSplitterBugFixed());
            Assert.That(!behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(!behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(!behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(!behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.70f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision2()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision2);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(!behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(!behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(!behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(!behaviourContext.IsSplitterBugFixed());
            Assert.That(!behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(!behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(!behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(!behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.70f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision3()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision3);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(!behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(!behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(!behaviourContext.IsSplitterBugFixed());
            Assert.That(!behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(!behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(!behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(!behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.70f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision4()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision4);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(!behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(!behaviourContext.IsSplitterBugFixed());
            Assert.That(!behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(!behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(!behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(!behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.75f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(1, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(1, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision5()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision5);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(!behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(!behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(2, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision6()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision6);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(!behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(2, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision7()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision7);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(!behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(2, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision8()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision8);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(!behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(3, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision9()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision9);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(!behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(3, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision10()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision10);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(!behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(4, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision11()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision11);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(!behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(5, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision12()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision12);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(!behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(5, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }

        [Test]
        public void TestRevision13()
        {
            BehaviourContext behaviourContext = new();

            behaviourContext.SetUserRevision(BehaviourContext.BaseRevisionMagic + BehaviourContext.Revision13);

            Assert.That(behaviourContext.IsAdpcmLoopContextBugFixed());
            Assert.That(behaviourContext.IsSplitterSupported());
            Assert.That(behaviourContext.IsLongSizePreDelaySupported());
            Assert.That(behaviourContext.IsAudioUsbDeviceOutputSupported());
            Assert.That(behaviourContext.IsFlushVoiceWaveBuffersSupported());
            Assert.That(behaviourContext.IsSplitterBugFixed());
            Assert.That(behaviourContext.IsElapsedFrameCountSupported());
            Assert.That(behaviourContext.IsDecodingBehaviourFlagSupported());
            Assert.That(behaviourContext.IsBiquadFilterEffectStateClearBugFixed());
            Assert.That(behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported());
            Assert.That(behaviourContext.IsWaveBufferVersion2Supported());
            Assert.That(behaviourContext.IsEffectInfoVersion2Supported());
            Assert.That(behaviourContext.UseMultiTapBiquadFilterProcessing());
            Assert.That(behaviourContext.IsNewEffectChannelMappingSupported());
            Assert.That(behaviourContext.IsBiquadFilterParameterForSplitterEnabled());
            Assert.That(behaviourContext.IsSplitterPrevVolumeResetSupported());

            ClassicAssert.AreEqual(0.80f, behaviourContext.GetAudioRendererProcessingTimeLimit());
            ClassicAssert.AreEqual(5, behaviourContext.GetCommandProcessingTimeEstimatorVersion());
            ClassicAssert.AreEqual(2, behaviourContext.GetPerformanceMetricsDataFormat());
        }
    }
}