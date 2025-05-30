namespace Hyjinx.HLE.HOS.Services.Settings;

enum ResultCode
{
    ModuleId = 105,
    ErrorCodeShift = 9,

    Success = 0,

    NullSettingsName = (201 << ErrorCodeShift) | ModuleId,
    NullSettingsKey = (202 << ErrorCodeShift) | ModuleId,
    NullSettingsValue = (203 << ErrorCodeShift) | ModuleId,
    NullSettingsValueBuffer = (205 << ErrorCodeShift) | ModuleId,
    NullSettingValueSizeBuffer = (208 << ErrorCodeShift) | ModuleId,
    NullDebugModeFlagBuffer = (209 << ErrorCodeShift) | ModuleId,
    SettingGroupNameHasZeroLength = (221 << ErrorCodeShift) | ModuleId,
    EmptySettingsItemKey = (222 << ErrorCodeShift) | ModuleId,
    SettingGroupNameIsTooLong = (241 << ErrorCodeShift) | ModuleId,
    SettingNameIsTooLong = (242 << ErrorCodeShift) | ModuleId,
    SettingGroupNameEndsWithDotOrContainsInvalidCharacters = (261 << ErrorCodeShift) | ModuleId,
    SettingNameEndsWithDotOrContainsInvalidCharacters = (262 << ErrorCodeShift) | ModuleId,
    NullLanguageCodeBuffer = (621 << ErrorCodeShift) | ModuleId,
    LanguageOutOfRange = (625 << ErrorCodeShift) | ModuleId,
    NullNetworkSettingsBuffer = (631 << ErrorCodeShift) | ModuleId,
    NullNetworkSettingsOutputCountBuffer = (632 << ErrorCodeShift) | ModuleId,
    NullBacklightSettingsBuffer = (641 << ErrorCodeShift) | ModuleId,
    NullBluetoothDeviceSettingBuffer = (651 << ErrorCodeShift) | ModuleId,
    NullBluetoothDeviceSettingOutputCountBuffer = (652 << ErrorCodeShift) | ModuleId,
    NullBluetoothEnableFlagBuffer = (653 << ErrorCodeShift) | ModuleId,
    NullBluetoothAFHEnableFlagBuffer = (654 << ErrorCodeShift) | ModuleId,
    NullBluetoothBoostEnableFlagBuffer = (655 << ErrorCodeShift) | ModuleId,
    NullBLEPairingSettingsBuffer = (656 << ErrorCodeShift) | ModuleId,
    NullBLEPairingSettingsEntryCountBuffer = (657 << ErrorCodeShift) | ModuleId,
    NullExternalSteadyClockSourceIDBuffer = (661 << ErrorCodeShift) | ModuleId,
    NullUserSystemClockContextBuffer = (662 << ErrorCodeShift) | ModuleId,
    NullNetworkSystemClockContextBuffer = (663 << ErrorCodeShift) | ModuleId,
    NullUserSystemClockAutomaticCorrectionEnabledFlagBuffer = (664 << ErrorCodeShift) | ModuleId,
    NullShutdownRTCValueBuffer = (665 << ErrorCodeShift) | ModuleId,
    NullExternalSteadyClockInternalOffsetBuffer = (666 << ErrorCodeShift) | ModuleId,
    NullAccountSettingsBuffer = (671 << ErrorCodeShift) | ModuleId,
    NullAudioVolumeBuffer = (681 << ErrorCodeShift) | ModuleId,
    NullForceMuteOnHeadphoneRemovedBuffer = (683 << ErrorCodeShift) | ModuleId,
    NullHeadphoneVolumeWarningCountBuffer = (684 << ErrorCodeShift) | ModuleId,
    InvalidAudioOutputMode = (687 << ErrorCodeShift) | ModuleId,
    NullHeadphoneVolumeUpdateFlagBuffer = (688 << ErrorCodeShift) | ModuleId,
    NullConsoleInformationUploadFlagBuffer = (691 << ErrorCodeShift) | ModuleId,
    NullAutomaticApplicationDownloadFlagBuffer = (701 << ErrorCodeShift) | ModuleId,
    NullNotificationSettingsBuffer = (702 << ErrorCodeShift) | ModuleId,
    NullAccountNotificationSettingsEntryCountBuffer = (703 << ErrorCodeShift) | ModuleId,
    NullAccountNotificationSettingsBuffer = (704 << ErrorCodeShift) | ModuleId,
    NullVibrationMasterVolumeBuffer = (711 << ErrorCodeShift) | ModuleId,
    NullNXControllerSettingsBuffer = (712 << ErrorCodeShift) | ModuleId,
    NullNXControllerSettingsEntryCountBuffer = (713 << ErrorCodeShift) | ModuleId,
    NullUSBFullKeyEnableFlagBuffer = (714 << ErrorCodeShift) | ModuleId,
    NullTVSettingsBuffer = (721 << ErrorCodeShift) | ModuleId,
    NullEDIDBuffer = (722 << ErrorCodeShift) | ModuleId,
    NullDataDeletionSettingsBuffer = (731 << ErrorCodeShift) | ModuleId,
    NullInitialSystemAppletProgramIDBuffer = (741 << ErrorCodeShift) | ModuleId,
    NullOverlayDispProgramIDBuffer = (742 << ErrorCodeShift) | ModuleId,
    NullIsInRepairProcessBuffer = (743 << ErrorCodeShift) | ModuleId,
    NullRequiresRunRepairTimeReviserBuffer = (744 << ErrorCodeShift) | ModuleId,
    NullDeviceTimezoneLocationNameBuffer = (751 << ErrorCodeShift) | ModuleId,
    NullPrimaryAlbumStorageBuffer = (761 << ErrorCodeShift) | ModuleId,
    NullUSB30EnableFlagBuffer = (771 << ErrorCodeShift) | ModuleId,
    NullUSBTypeCPowerSourceCircuitVersionBuffer = (772 << ErrorCodeShift) | ModuleId,
    NullBatteryLotBuffer = (781 << ErrorCodeShift) | ModuleId,
    NullSerialNumberBuffer = (791 << ErrorCodeShift) | ModuleId,
    NullLockScreenFlagBuffer = (801 << ErrorCodeShift) | ModuleId,
    NullColorSetIDBuffer = (803 << ErrorCodeShift) | ModuleId,
    NullQuestFlagBuffer = (804 << ErrorCodeShift) | ModuleId,
    NullWirelessCertificationFileSizeBuffer = (805 << ErrorCodeShift) | ModuleId,
    NullWirelessCertificationFileBuffer = (806 << ErrorCodeShift) | ModuleId,
    NullInitialLaunchSettingsBuffer = (807 << ErrorCodeShift) | ModuleId,
    NullDeviceNicknameBuffer = (808 << ErrorCodeShift) | ModuleId,
    NullBatteryPercentageFlagBuffer = (809 << ErrorCodeShift) | ModuleId,
    NullAppletLaunchFlagsBuffer = (810 << ErrorCodeShift) | ModuleId,
    NullWirelessLANEnableFlagBuffer = (1012 << ErrorCodeShift) | ModuleId,
    NullProductModelBuffer = (1021 << ErrorCodeShift) | ModuleId,
    NullNFCEnableFlagBuffer = (1031 << ErrorCodeShift) | ModuleId,
    NullECIDeviceCertificateBuffer = (1041 << ErrorCodeShift) | ModuleId,
    NullETicketDeviceCertificateBuffer = (1042 << ErrorCodeShift) | ModuleId,
    NullSleepSettingsBuffer = (1051 << ErrorCodeShift) | ModuleId,
    NullEULAVersionBuffer = (1061 << ErrorCodeShift) | ModuleId,
    NullEULAVersionEntryCountBuffer = (1062 << ErrorCodeShift) | ModuleId,
    NullLDNChannelBuffer = (1071 << ErrorCodeShift) | ModuleId,
    NullSSLKeyBuffer = (1081 << ErrorCodeShift) | ModuleId,
    NullSSLCertificateBuffer = (1082 << ErrorCodeShift) | ModuleId,
    NullTelemetryFlagsBuffer = (1091 << ErrorCodeShift) | ModuleId,
    NullGamecardKeyBuffer = (1101 << ErrorCodeShift) | ModuleId,
    NullGamecardCertificateBuffer = (1102 << ErrorCodeShift) | ModuleId,
    NullPTMBatteryLotBuffer = (1111 << ErrorCodeShift) | ModuleId,
    NullPTMFuelGaugeParameterBuffer = (1112 << ErrorCodeShift) | ModuleId,
    NullECIDeviceKeyBuffer = (1121 << ErrorCodeShift) | ModuleId,
    NullETicketDeviceKeyBuffer = (1122 << ErrorCodeShift) | ModuleId,
    NullSpeakerParameterBuffer = (1131 << ErrorCodeShift) | ModuleId,
    NullFirmwareVersionBuffer = (1141 << ErrorCodeShift) | ModuleId,
    NullFirmwareVersionDigestBuffer = (1142 << ErrorCodeShift) | ModuleId,
    NullRebootlessSystemUpdateVersionBuffer = (1143 << ErrorCodeShift) | ModuleId,
    NullMiiAuthorIDBuffer = (1151 << ErrorCodeShift) | ModuleId,
    NullFatalFlagsBuffer = (1161 << ErrorCodeShift) | ModuleId,
    NullAutoUpdateEnableFlagBuffer = (1171 << ErrorCodeShift) | ModuleId,
    NullExternalRTCResetFlagBuffer = (1181 << ErrorCodeShift) | ModuleId,
    NullPushNotificationActivityModeBuffer = (1191 << ErrorCodeShift) | ModuleId,
    NullServiceDiscoveryControlSettingBuffer = (1201 << ErrorCodeShift) | ModuleId,
    NullErrorReportSharePermissionBuffer = (1211 << ErrorCodeShift) | ModuleId,
    NullLCDVendorIDBuffer = (1221 << ErrorCodeShift) | ModuleId,
    NullConsoleSixAxisSensorAccelerationBiasBuffer = (1231 << ErrorCodeShift) | ModuleId,
    NullConsoleSixAxisSensorAngularVelocityBiasBuffer = (1232 << ErrorCodeShift) | ModuleId,
    NullConsoleSixAxisSensorAccelerationGainBuffer = (1233 << ErrorCodeShift) | ModuleId,
    NullConsoleSixAxisSensorAngularVelocityGainBuffer = (1234 << ErrorCodeShift) | ModuleId,
    NullConsoleSixAxisSensorAngularVelocityTimeBiasBuffer = (1235 << ErrorCodeShift) | ModuleId,
    NullConsoleSixAxisSensorAngularAccelerationBuffer = (1236 << ErrorCodeShift) | ModuleId,
    NullKeyboardLayoutBuffer = (1241 << ErrorCodeShift) | ModuleId,
    InvalidKeyboardLayout = (1245 << ErrorCodeShift) | ModuleId,
    NullWebInspectorFlagBuffer = (1251 << ErrorCodeShift) | ModuleId,
    NullAllowedSSLHostsBuffer = (1252 << ErrorCodeShift) | ModuleId,
    NullAllowedSSLHostsEntryCountBuffer = (1253 << ErrorCodeShift) | ModuleId,
    NullHostFSMountPointBuffer = (1254 << ErrorCodeShift) | ModuleId,
    NullAmiiboKeyBuffer = (1271 << ErrorCodeShift) | ModuleId,
    NullAmiiboECQVCertificateBuffer = (1272 << ErrorCodeShift) | ModuleId,
    NullAmiiboECDSACertificateBuffer = (1273 << ErrorCodeShift) | ModuleId,
    NullAmiiboECQVBLSKeyBuffer = (1274 << ErrorCodeShift) | ModuleId,
    NullAmiiboECQVBLSCertificateBuffer = (1275 << ErrorCodeShift) | ModuleId,
    NullAmiiboECQVBLSRootCertificateBuffer = (1276 << ErrorCodeShift) | ModuleId,
}