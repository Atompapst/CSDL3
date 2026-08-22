// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Input {
    /// <summary>SDL joystick axis limits.</summary>
    public static class JoystickConstants {
        /// <inheritdoc cref="Macros.JoystickAxisMax"/>
        public const short AxisMaximum = (short)Macros.JoystickAxisMax;
        /// <inheritdoc cref="Macros.JoystickAxisMin"/>
        public const short AxisMinimum = Macros.JoystickAxisMin;
    }
}
