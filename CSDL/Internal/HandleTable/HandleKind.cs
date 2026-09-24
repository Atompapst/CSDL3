// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    ///     Who is responsible for releasing a native resource.
    /// </summary>
    internal enum HandleKind : byte {
        /// <summary>This process must free the resource.</summary>
        Owned = 0,

        /// <summary>SDL frees the resource - a window surface, a camera frame, a tray entry.</summary>
        Borrowed = 1,
    }
}
