// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    public static class Power {
        /// <inheritdoc cref="CSDL.Internal.Docs.Power.GetPowerInfo"/>
        public static PowerState State {
            get {
                PowerState state = SDL.GetPowerInfo(out _, out _);
                return state;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Power.GetPowerInfo"/>
        public static PowerInfo Info {
            get {
                PowerState state = SDL.GetPowerInfo(out int seconds, out int percent);
                return new PowerInfo(state, seconds, percent);
            }
        }
    }

    public readonly record struct PowerInfo(
        PowerState State,
        int SecondsLeft,
        int PercentLeft
    );
}
