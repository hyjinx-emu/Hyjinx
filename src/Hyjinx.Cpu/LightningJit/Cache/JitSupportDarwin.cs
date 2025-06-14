using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hyjinx.Cpu.LightningJit.Cache;

[SupportedOSPlatform("macos")]
static partial class JitSupportDarwin
{
    [LibraryImport("libarmeilleure", EntryPoint = "armeilleure_jit_memcpy")]
    public static partial void Copy(IntPtr dst, IntPtr src, ulong n);

    [LibraryImport("libc", EntryPoint = "sys_icache_invalidate", SetLastError = true)]
    public static partial void SysIcacheInvalidate(IntPtr start, IntPtr len);
}