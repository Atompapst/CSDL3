// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    /// <summary>
    ///     A wrapper that other resources can be created against.
    /// </summary>
    internal interface IHandleOwner {
        /// <summary>This handle's identity, for resources SDL destroys together with it.</summary>
        OwnerId AsOwner { get; }
    }
}
