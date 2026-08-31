// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;

namespace CSDL.TTF {
    /// <summary>
    /// Entry point for the SDL_ttf subsystem: initialization and version info.
    /// </summary>
    public static partial class TTF {
        static TTF() {
            Init.OnQuit += Quit;
        }

        /// <summary>
        /// Initializes SDL_ttf if it hasn't been already. Called automatically by
        /// <see cref="Font"/> before it touches the library.
        /// </summary>
        internal static void EnsureInitialized() {
            SDL.Init().ThrowIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.Quit"/>
        internal static void Quit() {
            SDL.Quit();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.Version"/>
        public static int Version => SDL.Version();

        /// <inheritdoc cref="Macros.TtfMajorVersion"/>
        public static uint MajorVersion => Macros.TtfMajorVersion;

        /// <inheritdoc cref="Macros.StyleNormal"/>
        public static uint StyleNormal => Macros.StyleNormal;

        /// <inheritdoc cref="Macros.HintingNormal"/>
        public static uint HintingNormal => Macros.HintingNormal;

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetFreeTypeVersion"/>
        public static void GetFreeTypeVersion(out int major, out int minor, out int patch) {
            SDL.GetFreeTypeVersion(out major, out minor, out patch);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.GetHarfBuzzVersion"/>
        public static void GetHarfBuzzVersion(out int major, out int minor, out int patch) {
            SDL.GetHarfBuzzVersion(out major, out minor, out patch);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.TTF.WasInit"/>
        public static int WasInit() {
            return SDL.WasInit();
        }
    }
}
