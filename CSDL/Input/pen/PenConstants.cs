// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Input {
    /// <summary>Input IDs used by SDL for pen-simulated mouse and touch events.</summary>
    public static class PenConstants {
        /// <inheritdoc cref="Macros.PenMouseid"/>
        public static readonly MouseID MouseID = unchecked((uint)Macros.PenMouseid);
        /// <inheritdoc cref="Macros.PenTouchid"/>
        public static readonly TouchID TouchID = unchecked((ulong)Macros.PenTouchid);
    }
}
