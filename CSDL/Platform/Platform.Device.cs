// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL {
    public static partial class Platform {
        private const int FormFactorApiVersion = 3006000;

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetDeviceFormFactor"/>
        public static FormFactor FormFactor => Version.SdlVersion >= FormFactorApiVersion
            ? SDL.GetDeviceFormFactor()
            : GetLegacyFormFactor();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetDeviceFormFactorName"/>
        public static string? FormFactorName => GetFormFactorName(FormFactor);

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetDeviceFormFactorName"/>
        public static string? GetFormFactorName(FormFactor formFactor) {
            if (Version.SdlVersion >= FormFactorApiVersion) {
                return SDL.GetDeviceFormFactorName(formFactor).ToUtf8String();
            }

            return formFactor switch {
                FormFactor.Desktop => "SDL_FORMFACTOR_DESKTOP",
                FormFactor.Laptop => "SDL_FORMFACTOR_LAPTOP",
                FormFactor.Phone => "SDL_FORMFACTOR_PHONE",
                FormFactor.Tablet => "SDL_FORMFACTOR_TABLET",
                FormFactor.Console => "SDL_FORMFACTOR_CONSOLE",
                FormFactor.Handheld => "SDL_FORMFACTOR_HANDHELD",
                FormFactor.Watch => "SDL_FORMFACTOR_WATCH",
                FormFactor.Tv => "SDL_FORMFACTOR_TV",
                FormFactor.Headset => "SDL_FORMFACTOR_HEADSET",
                FormFactor.Car => "SDL_FORMFACTOR_CAR",
                _ => "SDL_FORMFACTOR_UNKNOWN",
            };
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.System.IsPhone"/>
        public static bool IsPhone => Version.SdlVersion >= FormFactorApiVersion && SDL.IsPhone();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.IsTablet"/>
        public static bool IsTablet => SDL.IsTablet();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.IsTV"/>
        public static bool IsTV => SDL.IsTV();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.IsChromebook"/>
        public static bool IsChromebook => SDL.IsChromebook();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.IsDeXMode"/>
        public static bool IsDeXMode => SDL.IsDeXMode();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.IsUbuntuTouch"/>
        public static bool IsUbuntuTouch => Version.SdlVersion >= FormFactorApiVersion && SDL.IsUbuntuTouch();

        /// <inheritdoc cref="CSDL.Internal.Docs.System.GetSandbox"/>
        public static Sandbox Sandbox => SDL.GetSandbox();

        /// <summary><c>true</c> if the app runs inside a sandbox (Flatpak, Snap, the macOS app sandbox, ...); otherwise, <c>false</c>.</summary>
        /// <seealso cref="CSDL.Internal.Docs.System.GetSandbox">SDL_GetSandbox</seealso>
        public static bool IsSandboxed => SDL.GetSandbox() != CSDL.Sandbox.None;

        private static FormFactor GetLegacyFormFactor() {
            if (SDL.IsTablet()) {
                return FormFactor.Tablet;
            }
            if (SDL.IsTV()) {
                return FormFactor.Tv;
            }

            return Id switch {
                PlatformId.XboxOne or PlatformId.XboxSeries or PlatformId.PS2 => FormFactor.Console,
                PlatformId.PSP or PlatformId.Vita or PlatformId.N3DS => FormFactor.Handheld,
                PlatformId.QNXNTO or PlatformId.WinGDK or PlatformId.Android or PlatformId.iOS or PlatformId.TvOS or PlatformId.VisionOS => FormFactor.Unknown,
                _ => FormFactor.Desktop,
            };
        }
    }
}
