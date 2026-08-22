// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Video {
    public static class ScreenSaver {
        /// <summary>
        ///  Check whether the screensaver is currently enabled.
        /// </summary>
        public static bool Enabled {
            get => SDL.ScreenSaverEnabled();
            set {
                if (value) EnableScreenSaver();
                else DisableScreenSaver();
            }
        }

        /// <summary>
        /// Allow the screen to be blanked by a screen saver.
        /// </summary>
        private static void EnableScreenSaver() {
            SDL.EnableScreenSaver().LogIfFalse();
        }

        /// <summary>
        /// Prevent the screen from being blanked by a screen saver.
        /// </summary>
        private static void DisableScreenSaver() {
            SDL.DisableScreenSaver().LogIfFalse();
        }
    }
}
