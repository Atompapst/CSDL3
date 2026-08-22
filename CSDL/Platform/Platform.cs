// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;
namespace CSDL {
    public static partial class Platform {
        private static readonly PlatformId Id;

        /// <summary>The platform name as reported by SDL (e.g. <c>"Windows"</c>, <c>"Linux"</c>, <c>"macOS"</c>).</summary>
        public static readonly string Name;

        /// <inheritdoc cref="CSDL.Internal.Docs.Platform.GetPlatform"/>
        static Platform() {
            string? platform = SDL.GetPlatform().ToUtf8String();
            Name = platform ?? "Unknown";
            Id = platform switch {
                "Windows" => PlatformId.Windows,
                "macOS" => PlatformId.macOS,
                "Linux" => PlatformId.Linux,
                "iOS" => PlatformId.iOS,
                "Android" => PlatformId.Android,
                "AIX" => PlatformId.AIX,
                "BSDI" => PlatformId.BSDi,
                "Emscripten" => PlatformId.Emscripten,
                "FreeBSD" => PlatformId.FreeBSD,
                "Haiku" => PlatformId.Haiku,
                "HP-UX" => PlatformId.HPUX,
                "Irix" => PlatformId.Irix,
                "MS-DOS" => PlatformId.MSDOS,
                "NetBSD" => PlatformId.NetBSD,
                "Nokia N-Gage" => PlatformId.NGage,
                "OpenBSD" => PlatformId.OpenBSD,
                "OS/2" => PlatformId.OS2,
                "OSF/1" => PlatformId.OSF,
                "QNX Neutrino" => PlatformId.QNXNTO,
                "RISC OS" => PlatformId.RISCOS,
                "Solaris" => PlatformId.Solaris,
                "Cygwin" => PlatformId.Cygwin,
                "WinGDK" => PlatformId.WinGDK,
                "Xbox One" => PlatformId.XboxOne,
                "Xbox Series X|S" => PlatformId.XboxSeries,
                "visionOS" => PlatformId.VisionOS,
                "tvOS" => PlatformId.TvOS,
                "PlayStation 2" => PlatformId.PS2,
                "PlayStation Portable" => PlatformId.PSP,
                "PlayStation Vita" => PlatformId.Vita,
                "Nintendo 3DS" => PlatformId.N3DS,
                "GNU/Hurd" => PlatformId.Hurd,
                "Atari MiNT" => PlatformId.AtariMiNT,
                "Managarm" => PlatformId.Managarm,
                _ => PlatformId.Unknown,
            };
        }

        /// <summary><c>true</c> if compiling for desktop Windows (Win32); otherwise, <c>false</c>.</summary>
        public static bool IsWIN32 => Id == PlatformId.Windows;

        /// <summary><c>true</c> if compiling for Windows, Microsoft GDK, or Xbox; otherwise, <c>false</c>.</summary>
        public static bool IsWindows => IsWIN32 || IsCygwin || IsWingdk || IsXboxone || IsXboxseries;

        /// <summary><c>true</c> if compiling for iOS, tvOS, or visionOS; otherwise, <c>false</c>.</summary>
        public static bool IsIOS => Id == PlatformId.iOS || IsTvos || IsVisionos;
        public static bool IsMacOS => Id == PlatformId.macOS;
        public static bool IsLinux => Id == PlatformId.Linux;
        public static bool IsAndroid => Id == PlatformId.Android;

        /// <summary><c>true</c> if running on an Apple platform (macOS, iOS, tvOS, or visionOS); otherwise, <c>false</c>.</summary>
        public static bool IsApple => IsMacOS || IsIOS || IsTvos || IsVisionos;

        public static bool IsAix => Id == PlatformId.AIX;
        public static bool IsBsdi => Id == PlatformId.BSDi;
        public static bool IsCygwin => Id == PlatformId.Cygwin;

        /// <summary><c>true</c> if running on MS-DOS; otherwise, <c>false</c>.</summary>
        public static bool IsDos => Id == PlatformId.MSDOS;

        public static bool IsEmscripten => Id == PlatformId.Emscripten;
        public static bool IsFreebsd => Id == PlatformId.FreeBSD;

        /// <summary><c>true</c> if running on Microsoft GDK, on any platform; otherwise, <c>false</c>.</summary>
        public static bool IsGdk => IsWingdk || IsXboxone || IsXboxseries;

        public static bool IsHaiku => Id == PlatformId.Haiku;
        public static bool IsHpux => Id == PlatformId.HPUX;
        public static bool IsHurd => Id == PlatformId.Hurd;
        public static bool IsIrix => Id == PlatformId.Irix;
        public static bool IsAtariMiNT => Id == PlatformId.AtariMiNT;
        public static bool IsManagarm => Id == PlatformId.Managarm;
        public static bool IsNetbsd => Id == PlatformId.NetBSD;
        public static bool IsNgage => Id == PlatformId.NGage;
        public static bool IsOpenbsd => Id == PlatformId.OpenBSD;
        public static bool IsOS2 => Id == PlatformId.OS2;
        public static bool IsOsf => Id == PlatformId.OSF;
        public static bool IsPS2 => Id == PlatformId.PS2;
        public static bool IsPsp => Id == PlatformId.PSP;

        /// <summary><c>true</c> if running on QNX Neutrino; otherwise, <c>false</c>.</summary>
        public static bool IsQnxnto => Id == PlatformId.QNXNTO;

        public static bool IsRiscos => Id == PlatformId.RISCOS;
        public static bool IsSolaris => Id == PlatformId.Solaris;
        public static bool IsTvos => Id == PlatformId.TvOS;

        /// <summary><c>true</c> if running on a Unix-like system; otherwise, <c>false</c>.</summary>
        /// <remarks>Other platforms, like Linux, might also report <c>true</c> in addition to their primary check.</remarks>
        public static bool IsUnix =>
            IsAix || IsAndroid || IsAtariMiNT || IsBsdi || IsCygwin || IsEmscripten ||
            IsFreebsd || IsHpux || IsHurd || IsIOS || IsIrix || IsLinux || IsMacOS ||
            IsManagarm || IsNetbsd || IsOpenbsd || IsOsf || IsQnxnto || IsSolaris;

        public static bool IsVisionos => Id == PlatformId.VisionOS;
        public static bool IsVita => Id == PlatformId.Vita;
        public static bool Is3DS => Id == PlatformId.N3DS;
        public static bool IsWingdk => Id == PlatformId.WinGDK;
        public static bool IsXboxone => Id == PlatformId.XboxOne;
        public static bool IsXboxseries => Id == PlatformId.XboxSeries;

        /// <summary><c>true</c> if the current running in a 32-bit process; otherwise, <c>false</c>.</summary>
        public static readonly bool Is32Bit = IntPtr.Size == 4;

        /// <summary><c>true</c> if the current running in a 64-bit process; otherwise, <c>false</c>.</summary>
        public static readonly bool Is64Bit = IntPtr.Size == 8;
    }
}
