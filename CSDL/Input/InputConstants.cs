// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Input {
    public static partial class Macros {
        /// <summary>The <see cref="MouseID"/> for mouse events simulated with touch input.</summary>
        public static readonly MouseID TouchMouseID = uint.MaxValue;

        /// <summary>The <see cref="TouchID"/> for touch events simulated with mouse input.</summary>
        public static readonly TouchID MouseTouchID = ulong.MaxValue;
    }
}
