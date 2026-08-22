// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.Extensions;
namespace CSDL.Audio {
    public static class Drivers {
        public static string[] AudioDrivers => GetAudioDrivers();

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetNumAudioDrivers"/>
        public static int Count => SDL.GetNumAudioDrivers();

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetAudioDriver"/>
        public static string GetAudioDriver(int index) {
            return SDL.GetAudioDriver(index).ToUtf8String() ?? string.Empty;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Audio.GetCurrentAudioDriver"/>
        public static string CurrentAudioDriver => SDL.GetCurrentAudioDriver().ToUtf8String() ?? string.Empty;

        private static string[] GetAudioDrivers() {
            int count = SDL.GetNumAudioDrivers();
            string[] drivers = new string[count];
            for (int i = 0; i < count; i++) {
                NativePtr<byte> driver = SDL.GetAudioDriver(i);
                drivers[i] = SDL.GetAudioDriver(i).ToUtf8String() ?? string.Empty;
            }
            return drivers;
        }

    }
}
