namespace Hyjinx.HLE.HOS.Applets.Browser;

enum WebArgTLVType : ushort
{
    InitialURL = 0x1,
    CallbackUrl = 0x3,
    CallbackableUrl = 0x4,
    ApplicationId = 0x5,
    DocumentPath = 0x6,
    DocumentKind = 0x7,
    SystemDataId = 0x8,
    ShareStartPage = 0x9,
    Whitelist = 0xA,
    NewsFlag = 0xB,
    UserID = 0xE,
    AlbumEntry0 = 0xF,
    ScreenShotEnabled = 0x10,
    EcClientCertEnabled = 0x11,
    PlayReportEnabled = 0x13,
    UnknownFlag0x14 = 0x14,
    UnknownFlag0x15 = 0x15,
    BootDisplayKind = 0x17,
    BackgroundKind = 0x18,
    FooterEnabled = 0x19,
    PointerEnabled = 0x1A,
    LeftStickMode = 0x1B,
    KeyRepeatFrame1 = 0x1C,
    KeyRepeatFrame2 = 0x1D,
    BootAsMediaPlayerInverted = 0x1E,
    DisplayUrlKind = 0x1F,
    BootAsMediaPlayer = 0x21,
    ShopJumpEnabled = 0x22,
    MediaAutoPlayEnabled = 0x23,
    LobbyParameter = 0x24,
    ApplicationAlbumEntry = 0x26,
    JsExtensionEnabled = 0x27,
    AdditionalCommentText = 0x28,
    TouchEnabledOnContents = 0x29,
    UserAgentAdditionalString = 0x2A,
    AdditionalMediaData0 = 0x2B,
    MediaPlayerAutoCloseEnabled = 0x2C,
    PageCacheEnabled = 0x2D,
    WebAudioEnabled = 0x2E,
    FooterFixedKind = 0x32,
    PageFadeEnabled = 0x33,
    MediaCreatorApplicationRatingAge = 0x34,
    BootLoadingIconEnabled = 0x35,
    PageScrollIndicatorEnabled = 0x36,
    MediaPlayerSpeedControlEnabled = 0x37,
    AlbumEntry1 = 0x38,
    AlbumEntry2 = 0x39,
    AlbumEntry3 = 0x3A,
    AdditionalMediaData1 = 0x3B,
    AdditionalMediaData2 = 0x3C,
    AdditionalMediaData3 = 0x3D,
    BootFooterButton = 0x3E,
    OverrideWebAudioVolume = 0x3F,
    OverrideMediaAudioVolume = 0x40,
    BootMode = 0x41,
    MediaPlayerUiEnabled = 0x43,
}