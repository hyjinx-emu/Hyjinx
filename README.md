<h1 align="center">
  <br>
  <a href="https://github.com/hyjinx-emu/Hyjinx"><img src="distribution/misc/Logo.svg" alt="Logo" width="150"></a>
  <br>
  <b>Hyjinx</b>
  <br>
  <sub><sup><b>(HI-JINGKS)</b></sup></sub>
  <br>
</h1>

<p align="center">
  Hyjinx is an open-source Nintendo Switch emulator, originally known as Ryujinx and created by gdkchan, written in C#.
  This emulator aims at providing excellent accuracy and performance, a user-friendly interface, and consistent builds.
  It was written from scratch, and development on the project began in September 2017.
</p>

[![build](https://github.com/hyjinx-emu/Hyjinx/actions/workflows/build.yml/badge.svg)](https://github.com/hyjinx-emu/Hyjinx/actions/workflows/build.yml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=hyjinx-emu_Hyjinx&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=hyjinx-emu_Hyjinx) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=hyjinx-emu_Hyjinx&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=hyjinx-emu_Hyjinx) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=hyjinx-emu_Hyjinx&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=hyjinx-emu_Hyjinx) [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=hyjinx-emu_Hyjinx&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=hyjinx-emu_Hyjinx)

## Usage

To run this emulator, your PC must be equipped with at least 8GiB of RAM;
failing to meet this requirement may result in a poor gameplay experience or unexpected crashes.

## Latest Build

These builds are compiled automatically for each commit on the main branch.
While we strive to ensure optimal stability and performance prior to pushing an update, our automated builds **may be unstable or completely broken**.

See the [Releases](https://github.com/hyjinx-emu/Hyjinx/releases) page for automatic builds for supported operating systems.

## Features
- **DMCA Compliance**

  The main goal of this fork was to provide a DMCA compliant emulator that allows the community to continue to tinker, while staying compliant with applicable laws. With this in mind, **this emulator is incapable of decrypting anything**. If any encrypted files are used, a warning or error will be shown to the user.

- **Audio**

  Audio output is entirely supported, audio input (microphone) isn't supported.
  We use C# wrappers for [OpenAL](https://openal-soft.org/), and [SDL2](https://www.libsdl.org/) & [libsoundio](http://libsound.io/) as fallbacks.

- **CPU**

  The CPU emulator, ARMeilleure, emulates an ARMv8 CPU and currently has support for most 64-bit ARMv8 and some of the ARMv7 (and older) instructions, including partial 32-bit support.
  It translates the ARM code to a custom IR, performs a few optimizations, and turns that into x86 code.
  There are three memory manager options available depending on the user's preference, leveraging both software-based (slower) and host-mapped modes (much faster).
  The fastest option (host, unchecked) is set by default.
  Hyjinx also features an optional Profiled Persistent Translation Cache, which essentially caches translated functions so that they do not need to be translated every time the game loads.
  The net result is a significant reduction in load times (the amount of time between launching a game and arriving at the title screen) for nearly every game.
  NOTE: This feature is enabled by default in the Options menu > System tab.
  You must launch the game at least twice to the title screen or beyond before performance improvements are unlocked on the third launch!
  These improvements are permanent and do not require any extra launches going forward.

- **GPU**

  The GPU emulator emulates the Switch's Maxwell GPU using either the OpenGL (version 4.5 minimum), Vulkan, or Metal (via MoltenVK) APIs through a custom build of OpenTK or Silk.NET respectively.
  There are currently six graphics enhancements available to the end user in Hyjinx: Disk Shader Caching, Resolution Scaling, Anti-Aliasing, Scaling Filters (including FSR), Anisotropic Filtering and Aspect Ratio Adjustment.
  These enhancements can be adjusted or toggled as desired in the GUI.

- **Input**

  We currently have support for keyboard, mouse, touch input, JoyCon input support, and nearly all controllers.
  Motion controls are natively supported in most cases; for dual-JoyCon motion support, DS4Windows or BetterJoy are currently required.
  In all scenarios, you can set up everything inside the input configuration menu.

- **DLC & Modifications**

  Hyjinx is able to manage add-on content/downloadable content through the GUI.
  Mods (romfs, exefs, and runtime mods such as cheats) are also supported;
  the GUI contains a shortcut to open the respective mods folder for a particular game.

- **Configuration**

  The emulator has settings for enabling or disabling some logging, remapping controllers, and more.
  You can configure all of them through the graphical interface or manually through the config file, `Config.json`, found in the user folder which can be accessed by clicking `Open Hyjinx Folder` under the File menu in the GUI.

<!--
## Contact
Currently contact is being kept intentionally limited. 
You may also review our [FAQ](https://github.com/hyjinx-emu/Hyjinx/wiki/Frequently-Asked-Questions).
-->

## License

This software is licensed under customized terms of the [MIT license](LICENSE.txt).
This project makes use of code authored by the libvpx project, licensed under BSD and the ffmpeg project, licensed under LGPLv3.
See [THIRDPARTY.md](distribution/legal/THIRDPARTY.md) for more details.

## Credits
- Ryujinx was used as the original source code for this emulator. This fork was later renamed to Hyjinx to ensure any issues caused by the project team wouldn't negatively effect the Ryujinx legacy or other forks.
- LibHac is used for our file-system.
- [AmiiboAPI](https://www.amiiboapi.com) is used in our Amiibo emulation.
- [ldn_mitm](https://github.com/spacemeowx2/ldn_mitm) is used for one of our available multiplayer modes.
- [ShellLink](https://github.com/securifybv/ShellLink) is used for Windows shortcut generation.
