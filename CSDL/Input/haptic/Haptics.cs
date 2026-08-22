// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL.Input {
    public static class Haptics {
        static Haptics() {
            Init.InitSubSystem(InitFlags.Haptic);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHaptics"/>
        public static HapticID[] GetConnected() {
            NativePtr<HapticID> ids = SDL.GetHaptics(out int count);
            if (ids.IsNull) {
                Error.LogError(nameof(GetConnected));
                return Array.Empty<HapticID>();
            }

            try {
                return count > 0 ? ids.ToManaged(count) : Array.Empty<HapticID>();
            }
            finally {
                Memory.Free(ids.Ptr);
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.GetHapticNameForID"/>
        public static string GetName(HapticID instanceID) {
            return SDL.GetHapticNameForID(instanceID).ToUtf8StringOrLog() ?? "Unknown Haptic Device";
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.IsJoystickHaptic"/>
        public static bool IsJoystickHaptic(JoystickDevice joystick) {
            ArgumentNullException.ThrowIfNull(joystick);
            return SDL.IsJoystickHaptic(joystick.Handle);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.IsMouseHaptic"/>
        public static bool IsMouseHaptic => SDL.IsMouseHaptic();

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.OpenHaptic"/>
        public static HapticDevice Open(HapticID id) {
            return new HapticDevice(id);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.OpenHapticFromJoystick"/>
        public static HapticDevice OpenFromJoystick(JoystickDevice joystick) {
            ArgumentNullException.ThrowIfNull(joystick);
            return new HapticDevice(SDL.OpenHapticFromJoystick(joystick.Handle).ThrowIfInvalid());
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Haptic.OpenHapticFromMouse"/>
        public static HapticDevice OpenFromMouse() {
            return new HapticDevice(SDL.OpenHapticFromMouse().ThrowIfInvalid());
        }
    }
}
